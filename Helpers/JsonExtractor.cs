using Newtonsoft.Json.Linq;

namespace SPOrchestratorAPI.Helpers;

public class JsonExtractor
{
    /// <summary>
    /// Extrae el valor de un campo a partir de un JSON y una ruta configurada.
    /// Si la ruta configurada contiene un punto (.) o empieza con "$", se interpreta como una ruta JSONPath.
    /// Si no, se buscará de forma recursiva en todo el objeto.
    /// </summary>
    /// <param name="jsonResponse">El JSON de respuesta.</param>
    /// <param name="configuredPath">La configuración, por ejemplo "RefDocumento" o "Nivel1.RefDocumento".</param>
    /// <returns>El valor extraído o null si no se encuentra.</returns>
    /// <exception cref="Exception">Si se encuentra más de una ocurrencia en el caso recursivo.</exception>
    public string ExtraerValor(string jsonResponse, string configuredPath)
    {
        JObject obj = JObject.Parse(jsonResponse);

        if (configuredPath.Contains(".") || configuredPath.StartsWith("$"))
        {
            var token = obj.SelectToken(configuredPath);
            return token?.ToString();
        }
        else
        {
            var tokens = obj.SelectTokens($"$..{configuredPath}").ToList();

            if (tokens.Count == 0)
            {
                return null;
            }
            else if (tokens.Count > 1)
            {
                throw new Exception($"Ambigüedad: se encontraron {tokens.Count} ocurrencias de '{configuredPath}'. Especifique la ruta completa (por ejemplo, Nivel1.{configuredPath}).");
            }
            else
            {
                return tokens[0].ToString();
            }
        }
    }
}