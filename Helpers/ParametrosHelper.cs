using System.Text.Json;
using System.Text.RegularExpressions;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Helper para validar y transformar una cadena de parámetros en formato JSON.
    /// Se espera que la cadena contenga valores separados por ';' sin espacios.
    /// Cada token debe cumplir:
    /// - Solo se permiten letras, dígitos, guiones (-) y guiones bajos (_).
    /// - Un guion o guion bajo debe estar intercalado entre caracteres alfanuméricos.
    /// - No se permiten las secuencias "-_" ni "_-".
    /// </summary>
    public static class ParametrosHelper
    {
        /// <summary>
        /// Valida que la cadena de parámetros cumpla el formato requerido y la transforma a JSON.
        /// Ejemplo de entrada: "fechadesde;fechahasta"
        /// Resultado: {"valor1":"fechadesde","valor2":"fechahasta"}
        /// </summary>
        /// <param name="parametros">Cadena de parámetros.</param>
        /// <returns>Cadena en formato JSON.</returns>
        /// <exception cref="ArgumentException">Si el formato no es correcto.</exception>
        public static string? ValidarYTransformar(string? parametros)
        {
            if (string.IsNullOrWhiteSpace(parametros))
            {
                return null;
            }
            
            if (parametros.Contains(" "))
            {
                throw new ArgumentException("No se permiten espacios en los parámetros. Separe los nombres con ';' sin espacios.");
            }
            
            var tokens = parametros.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                throw new ArgumentException("Debe proporcionar al menos un parámetro separado por ';'.");
            }
            
            var regex = new Regex("^(?!.*(?:-_|_-))[A-Za-z0-9](?:[A-Za-z0-9]|[-_](?=[A-Za-z0-9]))*$");

            var diccionario = new Dictionary<string, string>();
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (!regex.IsMatch(token))
                {
                    throw new ArgumentException($"El token '{token}' no cumple con el formato permitido. Se permiten letras, dígitos, guiones (-) y guiones bajos (_), sin espacios ni las secuencias '-_' o '_-'.");
                }
                diccionario[$"valor{i + 1}"] = token;
            }
            
            return JsonSerializer.Serialize(diccionario);
            
        }
    }
}
