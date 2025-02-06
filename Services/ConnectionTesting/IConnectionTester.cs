using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Services.ConnectionTesting
{
    public interface IConnectionTester
    {
        /// <summary>
        /// Prueba la conexión utilizando la cadena y el proveedor especificado.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión a la base de datos.</param>
        /// <param name="provider">Proveedor de base de datos.</param>
        /// <returns>Resultado del testeo de conexión.</returns>
        Task<ConnectionTestResult> TestConnectionAsync(string connectionString, DatabaseProvider provider);
    }
}