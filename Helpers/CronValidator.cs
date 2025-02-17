namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Utilidad para validar expresiones CRON.
    /// </summary>
    public static class CronValidator
    {
        /// <summary>
        /// Valida que la expresión CRON tenga 5 campos separados por espacios.
        /// </summary>
        /// <param name="cronExpression">La expresión CRON a validar.</param>
        /// <returns><c>true</c> si la expresión es válida; de lo contrario, <c>false</c>.</returns>
        public static bool IsValid(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                return false;
            }

            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 5;
        }
    }
}