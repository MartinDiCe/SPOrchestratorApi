using System.Reactive;

namespace SPOrchestratorAPI.Services.SPOrchestratorServices
{
    /// <summary>
    /// Define las operaciones para la ejecución programada de servicios.
    /// </summary>
    public interface IScheduledOrchestratorService
    {
        /// <summary>
        /// Ejecuta de forma programada un servicio basado en su configuración.
        /// </summary>
        /// <remarks>
        /// Este método:
        /// <list type="bullet">
        ///   <item>
        ///     <description>Valida que la configuración con <paramref name="servicioConfigId"/> exista y tenga <c>EsProgramado=true</c>.</description>
        ///   </item>
        ///   <item>
        ///     <description>Comprueba las fechas de <c>StartDate</c> y <c>EndDate</c> de la programación asociada.</description>
        ///   </item>
        ///   <item>
        ///     <description>Invoca de forma 100% reactiva al orquestador (chain) para ejecutar el SP, vista o endpoint.</description>
        ///   </item>
        ///   <item>
        ///     <description>Emite un único <see cref="Unit"/> al completar o notifica un error si falla.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <param name="servicioConfigId">
        /// Identificador de la configuración del servicio que se debe ejecutar.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{Unit}"/> que:
        /// <list type="bullet">
        ///   <item>
        ///     <description>Emite <see cref="Unit.Default"/> cuando la ejecución (y sus continuaciones) han finalizado correctamente.</description>
        ///   </item>
        ///   <item>
        ///     <description>Propaga una excepción si ocurre algún error durante la validación o la ejecución.</description>
        ///   </item>
        /// </list>
        /// </returns>
        IObservable<Unit> EjecutarProgramado(int servicioConfigId);
    }
}