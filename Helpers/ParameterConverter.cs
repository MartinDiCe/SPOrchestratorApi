using System.Text.Json;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Proporciona métodos para convertir parámetros provenientes de JSON a tipos nativos.
    /// </summary>
    public static class ParameterConverter
    {
        /// <summary>
        /// Convierte los valores del diccionario de parámetros a tipos nativos.
        /// </summary>
        /// <param name="parameters">Diccionario de parámetros a convertir.</param>
        /// <returns>Un nuevo diccionario con los valores convertidos.</returns>
        public static IDictionary<string, object> ConvertParameters(IDictionary<string, object>? parameters)
        {
            var convertedParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    object convertedValue;
                    if (kvp.Value is JsonElement jsonElem)
                    {
                        switch (jsonElem.ValueKind)
                        {
                            case JsonValueKind.String:
                                {
                                    string? s = jsonElem.GetString();
                                    convertedValue = string.IsNullOrWhiteSpace(s) ? DBNull.Value : (object)s;
                                    break;
                                }
                            case JsonValueKind.Number:
                                convertedValue = jsonElem.ToString();
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                convertedValue = jsonElem.GetBoolean();
                                break;
                            default:
                                convertedValue = jsonElem.ToString();
                                break;
                        }
                    }
                    else
                    {
                        convertedValue = kvp.Value ?? DBNull.Value;
                    }
                    convertedParams[kvp.Key] = convertedValue;
                }
            }

            return convertedParams;
        }
    }
}