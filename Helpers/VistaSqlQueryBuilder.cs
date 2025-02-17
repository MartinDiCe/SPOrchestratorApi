namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Construye consultas SQL para vistas agregando dinámicamente cláusulas WHERE.
    /// </summary>
    public static class VistaSqlQueryBuilder
    {
        /// <summary>
        /// Construye la consulta base para la vista y, si existen parámetros con valor, agrega la cláusula WHERE.
        /// </summary>
        /// <param name="baseViewName">Nombre de la vista SQL.</param>
        /// <param name="parameters">Diccionario de parámetros ya convertidos a tipos nativos.</param>
        /// <returns>La consulta SQL completa.</returns>
        public static string BuildQuery(string baseViewName, IDictionary<string, object> parameters)
        {
            string query = $"SELECT * FROM {baseViewName}";
            var whereClauses = new List<string>();

            foreach (var kvp in parameters)
            {
                // Solo incluir el parámetro en el WHERE si tiene un valor significativo.
                if (!(kvp.Value is string s && string.IsNullOrWhiteSpace(s)) && kvp.Value != DBNull.Value)
                {
                    whereClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                }
            }

            if (whereClauses.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", whereClauses);
            }

            return query;
        }
    }
}
