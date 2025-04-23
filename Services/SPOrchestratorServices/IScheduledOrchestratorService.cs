using System.Reactive;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    /// <summary>
    /// Define las operaciones para la ejecución programada de servicios.
    /// </summary>
    public interface IScheduledOrchestratorService
    {
        /// <summary>
        /// Invocado por Hangfire: lanza el pipeline y espera a que termine.
        /// </summary>
        Task EjecutarProgramadoAsync(string serviceName, int servicioConfigId);

        /// <summary>
        /// Pipeline reactivo puro (para pruebas manuales).
        /// </summary>
        IObservable<Unit> EjecutarProgramado(int servicioConfigId);
    }
}