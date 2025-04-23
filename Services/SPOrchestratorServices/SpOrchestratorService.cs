using System.Reactive.Linq;
using System.Text.Json;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Services.AuditServices;
using SPOrchestratorAPI.Services.EndpointServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using SPOrchestratorAPI.Services.StoreProcedureServices;
using SPOrchestratorAPI.Services.VistasSqlServices;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    /// <summary>
    /// Servicio que orquesta la ejecución de SP, vistas o endpoints por nombre,
    /// con auditoría opcional según configuración y flag de omisión.
    /// </summary>
    public class SpOrchestratorService : ISpOrchestratorService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServicioConfiguracionService _configService;
        private readonly IStoredProcedureService _storedProcedureService;
        private readonly IVistaSqlService _vistaSqlService;
        private readonly IEndpointService _endpointService;

        /// <summary>
        /// Constructor del SP Orchestrator.
        /// </summary>
        public SpOrchestratorService(
            IServiceScopeFactory scopeFactory,
            IServicioConfiguracionService configService,
            IStoredProcedureService storedProcedureService,
            IVistaSqlService vistaSqlService,
            IEndpointService endpointService)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _storedProcedureService = storedProcedureService ?? throw new ArgumentNullException(nameof(storedProcedureService));
            _vistaSqlService = vistaSqlService ?? throw new ArgumentNullException(nameof(vistaSqlService));
            _endpointService = endpointService ?? throw new ArgumentNullException(nameof(endpointService));
        }

        /// <inheritdoc />
        /// <param name="serviceName">Nombre del servicio configurado.</param>
        /// <param name="parameters">Parámetros de ejecución.</param>
        /// <param name="skipAudit">
        /// Si es true, omite el registro de auditoría incluso si la configuración lo solicita.
        /// </param>
        public IObservable<object> EjecutarPorNombreAsync(
            string serviceName,
            IDictionary<string, object>? parameters = null,
            bool skipAudit = false)
        {
            return Observable.Defer(() => Observable.FromAsync(async () =>
            {
                using var scope = _scopeFactory.CreateScope();

                var servicioService = scope.ServiceProvider.GetRequiredService<IServicioService>();
                var configService = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                var storedProcSvc = scope.ServiceProvider.GetRequiredService<IStoredProcedureService>();
                var vistaSvc = scope.ServiceProvider.GetRequiredService<IVistaSqlService>();
                var endpointSvc = scope.ServiceProvider.GetRequiredService<IEndpointService>();
                var auditoriaSvc = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                var startTime = DateTime.UtcNow;

                var servicio = await servicioService.GetByNameAsync(serviceName).FirstAsync();
                if (servicio == null)
                    throw new ResourceNotFoundException($"Servicio '{serviceName}' no encontrado.");

                var configs = await configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                if (configs == null || configs.Count == 0)
                    throw new ResourceNotFoundException($"Configuración no encontrada para '{serviceName}'.");

                var config = configs[0];
                if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                    throw new InvalidOperationException("Nombre de procedimiento/vista/endpoint no definido.");

                object resultData;

                switch (config.Tipo)
                {
                    case TipoConfiguracion.StoredProcedure:
                        resultData = await storedProcSvc
                            .EjecutarSpConRespuestaPorNombreAsync(serviceName, parameters)
                            .FirstAsync();
                        break;
                    case TipoConfiguracion.VistaSql:
                        resultData = await vistaSvc
                            .EjecutarVistaPorNombreAsync(serviceName, parameters)
                            .FirstAsync();
                        break;
                    case TipoConfiguracion.EndPoint:
                        resultData = await endpointSvc
                            .EjecutarEndpointPorNombreAsync(serviceName, parameters)
                            .FirstAsync();
                        break;
                    default:
                        throw new NotSupportedException("Tipo de configuración no soportado.");
                }

                var executionTime = (DateTime.UtcNow - startTime).TotalSeconds;

                if (config.GuardarRegistros && !skipAudit)
                {
                    var ejec = new ServicioEjecucion
                    {
                        ServicioId = servicio.Id,
                        ServicioConfiguracionId = config.Id,
                        ServicioDesencadenadorId = null,
                        FechaEjecucion = DateTime.UtcNow,
                        Duracion = executionTime,
                        Estado = true,
                        Parametros = JsonSerializer.Serialize(parameters),
                        Resultado = JsonSerializer.Serialize(resultData)
                    };
                    await auditoriaSvc.RegistrarEjecucionAsync(ejec);
                }

                return resultData;
            }));
        }
    }
}
