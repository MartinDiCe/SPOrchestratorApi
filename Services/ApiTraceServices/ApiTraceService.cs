using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories.ApiTraceRepositories;

namespace SPOrchestratorAPI.Services.ApiTraceServices
{
    /// <summary>
    /// Implementa la lógica de negocio para el registro de trazas de la API.
    /// </summary>
    public class ApiTraceService(IApiTraceRepository apiTraceRepository) : IApiTraceService
    {
        private readonly IApiTraceRepository _apiTraceRepository = apiTraceRepository ?? throw new ArgumentNullException(nameof(apiTraceRepository));

        /// <inheritdoc />
        public async Task<ApiTrace> CreateAsync(ApiTrace trace)
        {
            return await _apiTraceRepository.CreateAsync(trace);
        }
    }
}