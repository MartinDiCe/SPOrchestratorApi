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
    public class SpOrchestratorService(
        IServiceScopeFactory scopeFactory,
        IServicioConfiguracionService configService,
        IStoredProcedureService storedProcedureService,
        IVistaSqlService vistaSqlService,
        IEndpointService endpointService,
        IServicioService servicioService,
        IAuditoriaService auditoriaService)
        : ISpOrchestratorService
    {
        private readonly IServiceScopeFactory _scopeFactory =
            scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

        private readonly IServicioConfiguracionService _configService =
            configService ?? throw new ArgumentNullException(nameof(configService));

        private readonly IStoredProcedureService _storedProcedureService =
            storedProcedureService ?? throw new ArgumentNullException(nameof(storedProcedureService));

        private readonly IVistaSqlService _vistaSqlService =
            vistaSqlService ?? throw new ArgumentNullException(nameof(vistaSqlService));

        private readonly IEndpointService _endpointService =
            endpointService ?? throw new ArgumentNullException(nameof(endpointService));

        private readonly IServicioService _servicioService =
            servicioService ?? throw new ArgumentNullException(nameof(servicioService));

        private readonly IAuditoriaService _auditoriaService =
            auditoriaService ?? throw new ArgumentNullException(nameof(auditoriaService));

        public IObservable<object> EjecutarPorNombreAsync(string serviceName,
            IDictionary<string, object>? parameters = null)
        {
            return Observable.Defer(() => Observable.FromAsync(async () =>
            {
                // Creamos un nuevo scope para cada ejecución y evitar compartir instancias (por ejemplo, del DbContext)
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Obtenemos los servicios nuevos a partir del scope creado
                    var servicioService = scope.ServiceProvider.GetRequiredService<IServicioService>();
                    var configService = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();
                    var storedProcedureService = scope.ServiceProvider.GetRequiredService<IStoredProcedureService>();
                    var vistaSqlService = scope.ServiceProvider.GetRequiredService<IVistaSqlService>();
                    var endpointService = scope.ServiceProvider.GetRequiredService<IEndpointService>();
                    var auditoriaService = scope.ServiceProvider.GetRequiredService<IAuditoriaService>();

                    var startTime = DateTime.UtcNow;

                    // Buscar el servicio por nombre
                    var servicio = await servicioService.GetByNameAsync(serviceName).FirstAsync();
                    if (servicio == null)
                        throw new ResourceNotFoundException(
                            $"No se encontró un servicio con el nombre '{serviceName}'.");

                    // Obtener la configuración del servicio
                    var configs = await configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                    if (configs == null || configs.Count == 0)
                        throw new ResourceNotFoundException(
                            $"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");

                    var config = configs[0];
                    if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                        throw new InvalidOperationException(
                            "El nombre del stored procedure, vista o endpoint no está definido en la configuración.");

                    object resultData = null;

                    // Ejecutar según el tipo configurado
                    if (config.Tipo == TipoConfiguracion.StoredProcedure)
                    {
                        resultData = await storedProcedureService
                            .EjecutarSpConRespuestaPorNombreAsync(serviceName, parameters).FirstAsync();
                    }
                    else if (config.Tipo == TipoConfiguracion.VistaSql)
                    {
                        resultData = await vistaSqlService.EjecutarVistaPorNombreAsync(serviceName, parameters)
                            .FirstAsync();
                    }
                    else if (config.Tipo == TipoConfiguracion.EndPoint)
                    {
                        resultData = await endpointService.EjecutarEndpointPorNombreAsync(serviceName, parameters)
                            .FirstAsync();
                    }
                    else
                    {
                        throw new NotSupportedException("El tipo de configuración no es soportado.");
                    }

                    var executionTime = (DateTime.UtcNow - startTime).TotalSeconds;

                    // Registrar la auditoría si la configuración lo requiere
                    if (config.GuardarRegistros)
                    {
                        await auditoriaService.RegistrarEjecucionAsync(new ServicioEjecucion
                        {
                            ServicioId = servicio.Id,
                            ServicioConfiguracionId = config.Id,
                            ServicioDesencadenadorId = null,
                            FechaEjecucion = DateTime.UtcNow,
                            Duracion = executionTime,
                            Estado = true,
                            Parametros = System.Text.Json.JsonSerializer.Serialize(parameters),
                            Resultado = System.Text.Json.JsonSerializer.Serialize(resultData)
                        });
                    }

                    return resultData;
                }
            }));
        }
    }
}
