using SPOrchestratorAPI.Models.DTOs.ConnectionDtos;
using SPOrchestratorAPI.Services.ConnectionTestingServices;

namespace SPOrchestratorAPI.Services.ServicioConfiguracionServices
{
    public interface IServicioConfiguracionConnectionTestService
    {
        /// <summary>
        /// Prueba la conexión de la configuración identificada por el id dado.
        /// </summary>
        /// <param name="idConfiguracion">Id de la configuración.</param>
        /// <returns>Resultado del testeo de conexión.</returns>
        Task<ConnectionTestResultDto> TestConnectionAsync(int idConfiguracion);
    }
}