using SPOrchestratorAPI.Services.ConnectionTesting;

namespace SPOrchestratorAPI.Services.ServicioConfiguracionServices
{
    public interface IServicioConfiguracionConnectionTestService
    {
        /// <summary>
        /// Prueba la conexión de la configuración identificada por el id dado.
        /// </summary>
        /// <param name="idConfiguracion">Id de la configuración.</param>
        /// <returns>Resultado del testeo de conexión.</returns>
        Task<ConnectionTestResult> TestConnectionAsync(int idConfiguracion);
    }
}