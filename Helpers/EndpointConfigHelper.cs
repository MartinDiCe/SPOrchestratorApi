namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Helper para validar y parsear la configuración de endpoints.
    /// Permite claves: RequiereApiKey, ApiKey, TipoRequest y AuthScheme.
    /// </summary>
    public static class EndpointConfigHelper
    {
        private static readonly HashSet<string> ClavesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "RequiereApiKey",
            "ApiKey",
            "TipoRequest",
            "AuthScheme"
        };

        /// <summary>
        /// Valida y parsea la cadena de configuración para un endpoint.
        /// </summary>
        /// <param name="configString">
        /// Cadena con pares "clave=valor" separados por ';'.
        /// </param>
        /// <param name="logger">
        /// Logger opcional para trazas. Si se proporciona, registrará la configuración parseada.
        /// </param>
        /// <returns>Diccionario inmutable con la configuración.</returns>
        /// <exception cref="FormatException">
        /// Si la cadena está vacía, el formato es incorrecto o aparecen claves no permitidas.
        /// </exception>
        public static IReadOnlyDictionary<string, string> ParseAndValidateConfig(
            string configString,
            ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(configString))
                throw new FormatException(
                    "La cadena de configuración no puede estar vacía. " +
                    "Formato esperado: clave1=valor1;clave2=valor2;...");

            // Separar y limpiar entradas
            var entries = configString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();

            // Parsear a diccionario mutable
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                var parts = entry.Split('=', 2);
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
                    throw new FormatException(
                        $"Par inválido '{entry}'. Debe ser 'clave=valor'.");

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (!ClavesPermitidas.Contains(key))
                {
                    var validas = string.Join(", ", ClavesPermitidas);
                    throw new FormatException(
                        $"La clave '{key}' no es válida. Claves permitidas: {validas}.");
                }

                dict[key] = value;
            }

            // Debug log
            logger?.LogDebug("Configuración parseada: {@Config}", dict);

            // Retornamos copia inmutable
            return new Dictionary<string, string>(dict);
        }
    }
}
