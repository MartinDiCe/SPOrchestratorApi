﻿using System.Reactive.Linq;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Enums;
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
        IServicioService servicioService)
        : ISpOrchestratorService
    {
        public IObservable<object> EjecutarPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null)
        {
            return Observable.Defer(() => Observable.FromAsync(async () =>
            {
                var servicio = await servicioService.GetByNameAsync(serviceName).FirstAsync();
                if (servicio == null)
                {
                    throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");
                }

                var configs = await configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
                if (configs == null || configs.Count == 0)
                {
                    throw new ResourceNotFoundException($"No se encontró configuración para el servicio '{serviceName}' (ID: {servicio.Id}).");
                }
                var config = configs[0];
                if (string.IsNullOrWhiteSpace(config.NombreProcedimiento))
                {
                    throw new InvalidOperationException("El nombre del stored procedure o vista no está definido en la configuración.");
                }
                
                if (config.Tipo == TipoConfiguracion.StoredProcedure)
                {
                    return await storedProcedureService.EjecutarSpConRespuestaPorNombreAsync(serviceName, parameters).FirstAsync();
                }
                else if (config.Tipo == TipoConfiguracion.VistaSql)
                {
                    return await vistaSqlService.EjecutarVistaPorNombreAsync(serviceName, parameters).FirstAsync();
                }
                else
                {
                    throw new NotSupportedException("El tipo de configuración no es soportado.");
                }
            }));
        }
    }
}