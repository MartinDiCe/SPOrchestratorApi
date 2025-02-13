namespace SPOrchestratorAPI.Services.SPOrchestratorServices;

public interface ISpOrchestratorService
{
    IObservable<object> EjecutarPorNombreAsync(string serviceName, IDictionary<string, object>? parameters = null);
}
