using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;
using SPOrchestratorAPI.Services.ConnectionTesting;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.ServicioConfiguracionServices
{
    public class ServicioConfiguracionConnectionTestService(
        IServicioConfiguracionRepository repository,
        IConnectionTester connectionTester,
        ILoggerService<ServicioConfiguracionConnectionTestService> logger)
        : IServicioConfiguracionConnectionTestService
    {
        private readonly IServicioConfiguracionRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IConnectionTester _connectionTester = connectionTester ?? throw new ArgumentNullException(nameof(connectionTester));
        private readonly ILoggerService<ServicioConfiguracionConnectionTestService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<ConnectionTestResult> TestConnectionAsync(int idConfiguracion)
        {
            _logger.LogInfo($"Iniciando test de conexión para la configuración con ID {idConfiguracion}.");

            // Convertir el observable a Task y obtener el primer valor
            var config = await _repository.GetByIdAsync(idConfiguracion).FirstAsync();
            if (config == null)
            {
                throw new ResourceNotFoundException($"No se encontró configuración con ID {idConfiguracion}.");
            }

            _logger.LogInfo($"Se obtuvo la configuración. Proveedor: {config.Provider}, Cadena: {config.ConexionBaseDatos}");

            var result = await _connectionTester.TestConnectionAsync(config.ConexionBaseDatos, config.Provider);

            if (result.IsSuccess)
            {
                _logger.LogInfo("Test de conexión exitoso.");
            }
            else
            {
                _logger.LogWarning($"Test de conexión fallido: {result.ExceptionMessage}");
            }

            return result;
        }
    }
}
