using System.Reactive;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices;

public interface IScheduledOrchestratorService
{
    /// <summary>
    /// Ejecuta de forma programada un servicio, en modo 100% reactivo, validando
    /// las fechas y la bandera <c>EsProgramado</c> de su configuración asociada.
    /// </summary>
    /// <param name="servicioConfigId">ID de la configuración que se desea ejecutar.</param>
    /// <returns>
    /// Un <see cref="IObservable{Unit}"/> que emite un valor al completar (o lanza excepción si falla).
    /// </returns>
    IObservable<Unit> EjecutarProgramado(int servicioConfigId);
}