using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ApiTraceRepositories
{
    /// <summary>
    /// Define las operaciones para el registro de trazas de la API.
    /// </summary>
    public interface IApiTraceRepository
    {
        /// <summary>
        /// Registra una nueva traza de la API.
        /// </summary>
        /// <param name="trace">Entidad de traza a crear.</param>
        /// <returns>La traza registrada.</returns>
        Task<ApiTrace> CreateAsync(ApiTrace trace);
    }
}