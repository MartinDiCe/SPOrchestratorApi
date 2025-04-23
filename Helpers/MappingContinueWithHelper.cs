using System.Reflection;
using System.Text.Json;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Helper para transformar el resultado de un servicio en el conjunto de
    /// parámetros de su <c>Continue-With</c>.
    /// <para>
    /// Soporta:
    /// <list type="bullet">
    ///   <item>Valores fijos con el prefijo “<c>+</c>”.</item>
    ///   <item>JSONPath mediante <see cref="JsonExtractor"/>.</item>
    ///   <item>Resultado como <see cref="string"/> JSON (objeto o array).</item>
    ///   <item><see cref="JsonElement"/>, <see cref="IDictionary{TKey,TValue}"/> y reflexión.</item>
    /// </list>
    /// Todo de forma <b>case-insensitive</b>.
    /// </para>
    /// </summary>
    public static class MappingContinueWithHelper
    {
        #region Parse & Validate
        /// <inheritdoc cref="ParseMapping(string)"/>
        public static IDictionary<string, string> ValidateAndParseMapping(
            string mappingString,
            IList<string>? parametrosEsperados = null)
        {
            var mapping = ParseMapping(mappingString);

            if (parametrosEsperados is { Count: > 0 })
            {
                foreach (var (_, paramDestino) in mapping)
                {
                    if (paramDestino.StartsWith("+")) continue;

                    if (!parametrosEsperados.Contains(
                            paramDestino, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new FormatException(
                            $"El parámetro de destino «{paramDestino}» " +
                            $"no está entre los esperados.");
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Parsea la cadena de mapeo (<c>"Campo=Param;Otro=OtroParam"</c>)
        /// a un diccionario clave→valor.
        /// </summary>
        /// <exception cref="FormatException">Formato incorrecto.</exception>
        public static IDictionary<string, string> ParseMapping(string mappingString)
        {
            if (string.IsNullOrWhiteSpace(mappingString))
                throw new FormatException("La cadena de mapeo no puede estar vacía.");

            return mappingString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToDictionary(
                    p => p.Split('=')[0].Trim(),
                    p => p.Split('=')[1].Trim(),
                    StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Transform
        /// <summary>
        /// Aplica el mapeo sobre <paramref name="result"/> devolviendo el
        /// diccionario de parámetros para la continuación.
        /// </summary>
        /// <param name="result">Resultado del servicio anterior.</param>
        /// <param name="mappingString">Cadena de mapeo.</param>
        /// <param name="serviceName">Nombre del servicio actual (solo logging).</param>
        /// <param name="logger">
        ///   (Opcional) Logger para trazas; se recomienda pasar desde el
        ///   middleware reactivo para <c>Debug</c>/<c>Warning</c>.
        /// </param>
        /// <exception cref="FormatException">
        /// Se lanza si algún campo no se encuentra.
        /// </exception>
        public static IDictionary<string, object> TransformarResultado(
            object result,
            string mappingString,
            string serviceName = "el servicio",
            ILogger? logger = null)
        {
            // LOG:
            logger?.LogDebug("⤷ TransformarResultado ({Service}) → {Mapping}",
                             serviceName, mappingString);

            var mapping = ParseMapping(mappingString);
            var output  = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var extractor = new JsonExtractor();

            foreach (var (sourceKey, targetParam) in mapping)
            {
                object? value = null;

                if (sourceKey.StartsWith("+"))                          // valor fijo
                {
                    value = sourceKey[1..];
                }
                else if (sourceKey.StartsWith("$") || sourceKey.Contains("["))   // JSONPath
                {
                    var json = result is string s ? s : JsonSerializer.Serialize(result);
                    value = extractor.ExtraerValor(json, sourceKey);
                }
                else
                {
                    value = ResolveFromKnownStructures(result, sourceKey);
                }

                if (value is null)
                {
                    throw new FormatException(
                        $"No se encontró la propiedad esperada «{targetParam}» " +
                        $"en el resultado del servicio «{serviceName}». " +
                        $"Combinación esperada: {sourceKey}={targetParam}.");
                }

                output[targetParam] = value;
            }

            // LOG:
            logger?.LogDebug("⤷ Transform final ({Service}) → {Payload}",
                             serviceName, output);

            return output;
        }
        #endregion

        #region Private helpers
        private static object? ResolveFromKnownStructures(object result, string sourceKey)
        {
            
            if (result is string jsonTxt)
            {
                var trim = jsonTxt.TrimStart();
                if (trim.StartsWith("{") || trim.StartsWith("["))
                {
                    using var doc = JsonDocument.Parse(trim);
                    var element = trim.StartsWith("{")
                        ? doc.RootElement
                        : doc.RootElement.ValueKind == JsonValueKind.Array &&
                          doc.RootElement.GetArrayLength() > 0
                              ? doc.RootElement[0]
                              : default;

                    if (element.ValueKind != JsonValueKind.Undefined)
                        return TryGet(element, sourceKey);
                }
            }

            // 2) JsonElement ------------------------------
            if (result is JsonElement je && je.ValueKind == JsonValueKind.Object)
                return TryGet(je, sourceKey);

            // 3) IDictionary<string, object> --------------
            if (result is IDictionary<string, object> dict &&
                dict.TryGetValue(sourceKey, out var valDict))
                return valDict;

            // 4) Reflexión --------------------------------
            var prop = result.GetType().GetProperty(
                           sourceKey,
                           BindingFlags.Public | BindingFlags.Instance |
                           BindingFlags.IgnoreCase);
            return prop?.GetValue(result);
        }

        /// <summary>
        /// Busca de forma case-insensitive una propiedad en un
        /// <see cref="JsonElement"/> de tipo objeto.
        /// </summary>
        private static object? TryGet(JsonElement element, string propName)
        {
            foreach (var p in element.EnumerateObject())
            {
                if (p.NameEquals(propName) ||
                    p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    return p.Value.ValueKind switch
                    {
                        JsonValueKind.Number => p.Value.TryGetInt64(out var l) ? l :
                                                p.Value.TryGetDecimal(out var d) ? d :
                                                p.Value.GetDouble(),
                        JsonValueKind.True   => true,
                        JsonValueKind.False  => false,
                        JsonValueKind.String => p.Value.GetString()!,
                        JsonValueKind.Null   => null,
                        _ => p.Value.ToString()
                    };
                }
            }
            return null;
        }
        #endregion
    }
}
