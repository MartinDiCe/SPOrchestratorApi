namespace SPOrchestratorAPI.Helpers
{
    public static class EndpointConfigHelper
    {
        
        private static readonly HashSet<string> ClavesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "RequiereApiKey",
            "ApiKey",
            "TipoRequest"
        };

        /// <summary>
        /// Valida y parsea la cadena de configuración para un endpoint.
        /// La cadena debe tener el formato: "clave1=valor1;clave2=valor2;..."
        /// Solo se permitirán las claves definidas en ClavesPermitidas.
        /// </summary>
        /// <param name="configString">La cadena de configuración a validar.</param>
        /// <returns>Un diccionario con los pares clave/valor.</returns>
        /// <exception cref="FormatException">
        /// Se lanza si se encuentra una clave no permitida o si el formato es incorrecto.
        /// </exception>
        public static IDictionary<string, string> ParseAndValidateConfig(string configString)
        {
            if (string.IsNullOrWhiteSpace(configString))
            {
                throw new FormatException("La cadena de configuración no puede estar vacía. Formato esperado: clave1=valor1;clave2=valor2;...");
            }

            var pairs = configString.Split(';')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList();

            var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length != 2)
                {
                    throw new FormatException($"El par '{pair}' no está en el formato correcto. Se esperaba 'clave=valor'. Formato esperado: clave1=valor1;clave2=valor2;...");
                }

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                {
                    throw new FormatException($"El par '{pair}' contiene clave o valor vacío. Formato esperado: clave1=valor1;clave2=valor2;...");
                }

                if (!ClavesPermitidas.Contains(key))
                {
                    string clavesValidas = string.Join(", ", ClavesPermitidas);
                    throw new FormatException($"La clave '{key}' no es válida. Claves permitidas: {clavesValidas}. Formato esperado: clave1=valor1;clave2=valor2;...");
                }

                config[key] = value;
            }

            return config;
        }
    }
}
