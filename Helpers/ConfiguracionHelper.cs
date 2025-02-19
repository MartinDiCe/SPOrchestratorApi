using System.Text.Json;

namespace SPOrchestratorAPI.Helpers
{
    public static class ConfiguracionHelper
    {
        /// <summary>
        /// Extrae los nombres de los parámetros esperados desde la cadena JSON almacenada en la configuración.
        /// Se asume que el JSON es un diccionario de clave-valor y se retornan los valores.
        /// </summary>
        /// <param name="jsonParametros">La cadena JSON que define los parámetros.</param>
        /// <returns>Lista de parámetros esperados.</returns>
        /// <exception cref="FormatException">Si el JSON no se puede parsear.</exception>
        public static IList<string> ObtenerParametrosEsperados(string? jsonParametros)
        {
            if (string.IsNullOrWhiteSpace(jsonParametros))
                return new List<string>();

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonParametros);
                return dict?.Values != null ? new List<string>(dict.Values) : new List<string>();
            }
            catch (Exception ex)
            {
                throw new FormatException("No se pudo parsear la cadena de parámetros esperados.", ex);
            }
        }
    }
}