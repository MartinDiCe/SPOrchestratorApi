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
        IServicioConfiguracionService configService,
        IStoredProcedureService storedProcedureService,
        IVistaSqlService vistaSqlService,
        IEndpointService endpointService,
        IServicioService servicioService,
        IAuditoriaService auditoriaService)
        : ISpOrchestratorService
    {
        private readonly IServicioConfiguracionService _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        private readonly IStoredProcedureService _storedProcedureService = storedProcedureService ?? throw new ArgumentNullException(nameof(storedProcedureService));
        private readonly IVistaSqlService _vistaSqlService = vistaSqlService ?? throw new ArgumentNullException(nameof(vistaSqlService));
        private readonly IEndpointService _endpointService = endpointService ?? throw new ArgumentNullException(nameof(endpointService));
        private readonly IServicioService _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));
        private readonly IAuditoriaService _auditoriaService = auditoriaService ?? throw new ArgumentNullException(nameof(auditoriaService));

        public IObservable<object> EjecutarPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return Observable.Defer(() => Observable.FromAsync(async () =>
            {
                var startTime = DateTime.UtcNow;

                var servicio = await _servicioService.GetByNameAsync(serviceName).FirstAsync();
                if (servicio == null)
                    throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");

                var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                if (configs == null || configs.Count == 0)
                    throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");

                var config = configs[0];
                if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                    throw new InvalidOperationException("El nombre del stored procedure, vista o endpoint no está definido en la configuración.");

                object resultData = null;
                // Seleccionar la ejecución según el tipo configurado
                if (config.Tipo == TipoConfiguracion.StoredProcedure)
                {
                    resultData = await _storedProcedureService.EjecutarSpConRespuestaPorNombreAsync(serviceName, parameters).FirstAsync();
                }
                else if (config.Tipo == TipoConfiguracion.VistaSql)
                {
                    resultData = await _vistaSqlService.EjecutarVistaPorNombreAsync(serviceName, parameters).FirstAsync();
                }
                else if (config.Tipo == TipoConfiguracion.EndPoint)
                {
                    resultData = await _endpointService.EjecutarEndpointPorNombreAsync(serviceName, parameters).FirstAsync();
                }
                else
                {
                    throw new NotSupportedException("El tipo de configuración no es soportado.");
                }

                var executionTime = (DateTime.UtcNow - startTime).TotalSeconds;
                
                if (config.GuardarRegistros)
                {
                    await _auditoriaService.RegistrarEjecucionAsync(new ServicioEjecucion
                    {
                        ServicioId = servicio.Id,
                        ServicioConfiguracionId = config.Id,
                        FechaEjecucion = DateTime.UtcNow,
                        Duracion = executionTime,
                        Estado = true, // o false según corresponda
                        Parametros = JsonSerializer.Serialize(parameters),
                        Resultado = JsonSerializer.Serialize(resultData)
                    });
                }

                return resultData;
            }));
        }
    }
}
