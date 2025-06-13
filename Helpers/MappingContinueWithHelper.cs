using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Helper para transformar el resultado de un servicio en un payload
    /// de parámetros para su Continue-With, soportando tipos dinámicos.
    /// </summary>
    public static class MappingContinueWithHelper
    {
        #region Parse & Validate
        /// <inheritdoc cref="ParseMapping(string, ILogger?)"/>
        public static IDictionary<string, string> ValidateAndParseMapping(
            string mappingString,
            IList<string>? parametrosEsperados = null)
        {
            var mapping = ParseMapping(mappingString, null);

            if (parametrosEsperados is { Count: > 0 })
            {
                foreach (var (_, rawDestino) in mapping)
                {
                    // Extrae nombre sin tipo
                    var m = Regex.Match(rawDestino, "^([^(]+)");
                    var destino = m.Success ? m.Groups[1].Value : rawDestino;
                    if (destino.StartsWith("+")) continue;

                    if (!parametrosEsperados.Contains(
                            destino, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new FormatException(
                            $"El parámetro de destino «{destino}» " +
                            "no está entre los esperados.");
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Parsea la cadena de mapeo (<c>"Campo=Param(tipo);Otro=OtroParam"</c>)
        /// a un diccionario clave→valor donde el valor puede incluir la especificación de tipo.
        /// </summary>
        /// <param name="mappingString">Cadena de mapeo.</param>
        /// <param name="logger">Logger para advertencias de tipo desconocido.</param>
        /// <returns>Diccionario fuente→destino(raw), raw puede ser "destino(tipo)" o "destino".</returns>
        public static IDictionary<string, string> ParseMapping(
            string mappingString,
            ILogger? logger)
        {
            if (string.IsNullOrWhiteSpace(mappingString))
                throw new FormatException("La cadena de mapeo no puede estar vacía.");

            return mappingString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToDictionary(
                    p => p.Split('=')[0].Trim(),
                    p => {
                        var raw = p.Split('=')[1].Trim();
                        // valida formato destino(tipo)
                        var m = Regex.Match(raw, "^([^()]+)(?:\\((\\w+)\\))?$");
                        if (!m.Success)
                            throw new FormatException($"Destino mal formado '{raw}'");
                        var tipoName = m.Groups[2].Success ? m.Groups[2].Value.ToLowerInvariant() : "string";
                        if (!new[] { "int","bool","decimal","datetime","string" }.Contains(tipoName))
                        {
                            logger?.LogWarning(
                                "Tipo '{Type}' en mapeo '{Raw}' no reconocido; usando 'string'.",
                                tipoName, raw);
                        }
                        return raw;
                    },
                    StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Transform
        /// <summary>
        /// Aplica el mapeo sobre <paramref name="result"/>, parsea tipos y devuelve
        /// el diccionario de parámetros para la continuación. Omite valores null.
        /// </summary>
        public static IDictionary<string, object> TransformarResultado(
            object result,
            string mappingString,
            string serviceName = "el servicio",
            ILogger? logger = null)
        {
            logger?.LogDebug("⤷ TransformarResultado ({Service}) → {Mapping}",
                             serviceName, mappingString);

            var mapping = ParseMapping(mappingString, logger);
            var output  = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var extractor = new JsonExtractor();

            foreach (var (sourceKey, rawDestino) in mapping)
            {
                // extrae nombre y tipo
                var m = Regex.Match(rawDestino, "^([^()]+)(?:\\((\\w+)\\))?");
                var destino = m.Groups[1].Value;
                var tipoName = m.Groups[2].Success ? m.Groups[2].Value.ToLowerInvariant() : "string";

                object? rawVal;
                if (sourceKey.StartsWith("+"))
                {
                    rawVal = sourceKey[1..];
                }
                else if (sourceKey.StartsWith("$") || sourceKey.Contains("["))
                {
                    var json = result is string s ? s : JsonSerializer.Serialize(result);
                    rawVal   = extractor.ExtraerValor(json, sourceKey);
                }
                else
                {
                    rawVal = ResolveFromKnownStructures(result, sourceKey);
                }

                if (rawVal is null) continue; // omitir nulls

                try
                {
                    var finalVal = ConvertByName(rawVal, tipoName);
                    output[destino] = finalVal;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex,
                        "Error convirtiendo '{Src}' a tipo {Type} en {Svc}",
                        sourceKey, tipoName, serviceName);
                    throw;
                }
            }

            logger?.LogDebug("⤷ Payload final ({Service}) → {@Payload}", serviceName, output);
            return output;
        }
        #endregion

        #region Converters & Resolve
        private static object ConvertByName(object raw, string tipoName)
        {
            return tipoName switch
            {
                "int"      => Convert.ToInt32(raw, CultureInfo.InvariantCulture),
                "bool"     => raw is bool b ? b : bool.Parse(raw.ToString()!),
                "decimal"  => Convert.ToDecimal(raw, CultureInfo.InvariantCulture),
                "datetime" => raw is DateTime dt ? dt : DateTime.Parse(raw.ToString()!, CultureInfo.InvariantCulture),
                _           => raw.ToString()!, // string por defecto
            };
        }

        private static object? ResolveFromKnownStructures(object result, string key)
        {
            if (result is string txt)
            {
                var t = txt.TrimStart();
                if (t.StartsWith("{") || t.StartsWith("["))
                {
                    using var doc = JsonDocument.Parse(t);
                    var elem = t.StartsWith("{")
                        ? doc.RootElement
                        : doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0
                            ? doc.RootElement[0]
                            : default;
                    if (elem.ValueKind != JsonValueKind.Undefined)
                        return TryGet(elem, key);
                }
            }

            if (result is JsonElement je && je.ValueKind == JsonValueKind.Object)
                return TryGet(je, key);

            if (result is IDictionary<string, object> dict && dict.TryGetValue(key, out var v))
                return v;

            var prop = result.GetType()
                             .GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return prop?.GetValue(result);
        }

        private static object? TryGet(JsonElement element, string propName)
        {
            foreach (var p in element.EnumerateObject())
            {
                if (p.NameEquals(propName) || p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    return p.Value.ValueKind switch
                    {
                        JsonValueKind.Number => p.Value.TryGetInt64(out var l) ? l : (object)p.Value.GetDouble(),
                        JsonValueKind.True   => true,
                        JsonValueKind.False  => false,
                        JsonValueKind.String => p.Value.GetString()!,
                        JsonValueKind.Null   => null,
                        _                    => p.Value.ToString()
                    };
                }
            }
            return null;
        }
        #endregion
    }
}
