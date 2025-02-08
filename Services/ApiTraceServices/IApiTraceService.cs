using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Services.ApiTraceServices
{
    /// <summary>
    /// Define las operaciones relacionadas con el registro de trazas de la API.
    /// </summary>
    public interface IApiTraceService
    {
        /// <summary>
        /// Registra una nueva traza de la API.
        /// </summary>
        /// <param name="trace">Entidad de traza a registrar.</param>
        /// <returns>La traza registrada.</returns>
        Task<ApiTrace> CreateAsync(ApiTrace trace);
    }
}