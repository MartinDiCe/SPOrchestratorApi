using SPOrchestratorAPI.Models.DTOs.ConnectionDtos;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Services.ConnectionTestingServices
{
    public interface IConnectionTesterService
    {
        /// <summary>
        /// Prueba la conexión utilizando la cadena y el proveedor especificado.
        /// </summary>
        /// <param name="connectionString">Cadena de conexión a la base de datos.</param>
        /// <param name="provider">Proveedor de base de datos.</param>
        /// <returns>Resultado del testeo de conexión.</returns>
        Task<ConnectionTestResultDto> TestConnectionAsync(string connectionString, DatabaseProvider provider);
    }
}