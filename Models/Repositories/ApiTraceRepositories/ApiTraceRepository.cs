using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ApiTraceRepositories
{
    /// <summary>
    /// Implementa el repositorio para el registro de trazas de la API utilizando Entity Framework Core.
    /// </summary>
    public class ApiTraceRepository : IApiTraceRepository
    {
        private readonly ApplicationDbContext _context;

        public ApiTraceRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<ApiTrace> CreateAsync(ApiTrace trace)
        {
            if (trace == null)
            {
                throw new ArgumentNullException(nameof(trace));
            }

            _context.ApiTraces.Add(trace);
            await _context.SaveChangesAsync();
            return trace;
        }
    }
}