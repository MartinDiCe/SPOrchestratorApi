using System.Data;
using System.Reactive.Linq;
using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;

namespace SPOrchestratorAPI.Services.VistasSqlServices;

public interface IVistaSqlService
{
    /// <summary>
    /// Ejecuta una consulta sobre una vista SQL utilizando la configuración asociada al servicio.
    /// </summary>
    /// <param name="serviceName">El nombre del servicio (y por ende de la vista) a consultar.</param>
    /// <param name="parameters">Parámetros opcionales para la consulta.</param>
    /// <returns>Un observable con el resultado de la consulta.</returns>
    IObservable<object> EjecutarVistaPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null);
    
}
