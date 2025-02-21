using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Provee métodos para validar y parsear la cadena de mapeo de continuación.
    /// El formato esperado es:
    /// "CampoResultado=ParametroContinuacion;OtroCampo=OtroParametro"
    /// Se permiten valores fijos indicando un prefijo, por ejemplo: "+ValorFijo=ParametroContinuacion".
    /// </summary>
    public static class MappingContinueWithHelper
    {
        public static IDictionary<string, string> ParseMapping(string mappingString)
        {
            if (string.IsNullOrWhiteSpace(mappingString))
                throw new FormatException("La cadena de mapeo no puede estar vacía.");

            mappingString = mappingString.Trim();
            var pairs = mappingString.Split(';')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in pairs)
            {
                var parts = pair.Trim().Split('=');
                if (parts.Length != 2)
                    throw new FormatException(
                        $"El par '{pair}' no está en el formato correcto. Se esperaba 'clave=valor'.");

                string clave = parts[0].Trim();
                string valor = parts[1].Trim();
                if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(valor))
                    throw new FormatException($"El par '{pair}' contiene clave o valor vacío.");

                mapping[clave] = valor;
            }

            return mapping;
        }

        public static IDictionary<string, string> ValidateAndParseMapping(string mappingString,
            IList<string>? parametrosEsperados = null)
        {
            var mapping = ParseMapping(mappingString);
            if (parametrosEsperados != null && parametrosEsperados.Any())
            {
                foreach (var kvp in mapping)
                {
                    if (kvp.Value.StartsWith("+"))
                        continue;
                    if (!parametrosEsperados.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new FormatException(
                            $"El parámetro de destino '{kvp.Value}' no se encuentra entre los parámetros esperados.");
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Transforma el resultado de un servicio en un diccionario de parámetros para la continuación,
        /// aplicando la cadena de mapeo definida.
        /// 
        /// El método espera que <paramref name="result"/> sea un objeto que permita acceder a sus valores
        /// mediante propiedades o que sea un <see cref="IDictionary{string, object}"/>.
        /// </summary>
        /// <param name="result">El resultado obtenido del servicio previo.</param>
        /// <param name="mappingString">
        /// La cadena de mapeo que define la transformación, con el formato:
        /// "CampoResultado=ParametroContinuacion;OtroCampo=OtroParametro".
        /// Se permiten valores fijos usando el prefijo '+' en el lado de la clave.
        /// </param>
        /// <param name="serviceName">
        /// (Opcional) El nombre del servicio en ejecución, para incluirlo en el mensaje de error.
        /// </param>
        /// <returns>
        /// Un diccionario donde la clave es el nombre del parámetro de continuación y el valor es obtenido
        /// desde <paramref name="result"/> o es un valor fijo.
        /// </returns>
        /// <exception cref="FormatException">
        /// Se lanza si no se encuentra la propiedad esperada en el resultado.
        /// </exception>
        public static IDictionary<string, object> TransformarResultado(object result, string mappingString,
            string serviceName = "el servicio")
        {
            var mapping = ParseMapping(mappingString);
            var output = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var extractor = new JsonExtractor(); // Usamos nuestro extractor de JSON

            foreach (var kvp in mapping)
            {
                string sourceKey = kvp.Key;
                string targetParam = kvp.Value;
                object? value = null;
                if (sourceKey.StartsWith("+"))
                {
                    value = sourceKey.Substring(1);
                }
                else
                {
                    // Si el sourceKey parece ser una ruta JSONPath (por ejemplo, comienza con '$' o contiene [*])
                    if (sourceKey.StartsWith("$") || sourceKey.Contains("["))
                    {
                        // Se espera que el 'result' sea un JSON en string
                        string json = result is string
                            ? (string)result
                            : System.Text.Json.JsonSerializer.Serialize(result);
                        value = extractor.ExtraerValor(json, sourceKey);
                        if (value == null)
                        {
                            throw new FormatException(
                                $"No se encontró la ruta JSONPath '{sourceKey}' en el resultado del servicio '{serviceName}'. Combinación esperada: {sourceKey}={targetParam}.");
                        }
                    }
                    else
                    {
                        if (result is IDictionary<string, object> dict)
                        {
                            if (!dict.TryGetValue(sourceKey, out value))
                            {
                                throw new FormatException(
                                    $"No se encontró la propiedad esperada '{targetParam}' en el resultado del servicio '{serviceName}'. Combinación esperada: {sourceKey}={targetParam}.");
                            }
                        }
                        else
                        {
                            PropertyInfo? prop = result.GetType().GetProperty(sourceKey,
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (prop != null)
                            {
                                value = prop.GetValue(result);
                            }
                            else
                            {
                                throw new FormatException(
                                    $"No se encontró la propiedad esperada '{targetParam}' en el resultado del servicio '{serviceName}'. Combinación esperada: {sourceKey}={targetParam}.");
                            }
                        }
                    }
                }

                output[targetParam] = value!;
            }

            return output;
        }
    }
}
