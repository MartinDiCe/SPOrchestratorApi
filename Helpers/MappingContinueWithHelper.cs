using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        /// <summary>
        /// Valida y parsea la cadena de mapeo.
        /// </summary>
        /// <param name="mappingString">La cadena de mapeo a validar.</param>
        /// <returns>
        /// Un diccionario en el que la clave es el campo de origen (o valor fijo si empieza con '+') 
        /// y el valor es el parámetro de destino.
        /// </returns>
        /// <exception cref="FormatException">
        /// Se lanza si el formato de la cadena no es válido.
        /// </exception>
        public static IDictionary<string, string> ParseMapping(string mappingString)
        {
            if (string.IsNullOrWhiteSpace(mappingString))
                throw new FormatException("La cadena de mapeo no puede estar vacía.");

            // Quitar espacios al inicio y al final.
            mappingString = mappingString.Trim();

            // Separamos los pares por ';'. 
            // Se ignoran entradas vacías (por ejemplo, si termina en ';')
            var pairs = mappingString.Split(';')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in pairs)
            {
                
                var parts = pair.Trim().Split('=');

                if (parts.Length != 2)
                    throw new FormatException($"El par '{pair}' no está en el formato correcto. Se esperaba 'clave=valor'.");

                string clave = parts[0].Trim();
                string valor = parts[1].Trim();

                // Validamos que ni clave ni valor estén vacíos
                if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(valor))
                    throw new FormatException($"El par '{pair}' contiene clave o valor vacío.");

                mapping[clave] = valor;
            }

            return mapping;
        }

        /// <summary>
        /// Valida la cadena de mapeo y la compara opcionalmente con la lista de parámetros esperados.
        /// </summary>
        /// <param name="mappingString">La cadena de mapeo a validar.</param>
        /// <param name="parametrosEsperados">
        /// Lista de nombres de parámetros que se esperan en el servicio de continuación.
        /// Si se proporciona, se verificará que cada valor destino esté en esta lista.
        /// </param>
        /// <returns>El diccionario parseado del mapeo.</returns>
        /// <exception cref="FormatException">
        /// Se lanza si el formato es incorrecto o si algún valor destino no se encuentra en la lista de esperados.
        /// </exception>
        public static IDictionary<string, string> ValidateAndParseMapping(string mappingString, IList<string>? parametrosEsperados = null)
        {
            var mapping = ParseMapping(mappingString);

            if (parametrosEsperados != null && parametrosEsperados.Any())
            {
                foreach (var kvp in mapping)
                {
                    // Si el valor destino comienza con '+' se considera un valor fijo, por lo que se omite la validación.
                    if (kvp.Value.StartsWith("+"))
                        continue;

                    if (!parametrosEsperados.Contains(kvp.Value, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new FormatException($"El parámetro de destino '{kvp.Value}' no se encuentra entre los parámetros esperados.");
                    }
                }
            }

            return mapping;
        }
    }
}
