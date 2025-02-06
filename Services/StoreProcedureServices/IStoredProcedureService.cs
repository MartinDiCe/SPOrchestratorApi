namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    public interface IStoredProcedureService
    {
        /// <summary>
        /// Ejecuta un procedimiento almacenado de forma asíncrona y reactiva.
        /// </summary>
        /// <param name="spName">Nombre del procedimiento almacenado.</param>
        /// <param name="parameters">
        /// Diccionario opcional con los parámetros a enviar al procedimiento.  
        /// Si es nulo o vacío, se ejecutará sin parámetros.
        /// </param>
        /// <returns>
        /// Un <see cref="IObservable{T}"/> que emite un <see cref="int"/> con el número de filas afectadas.
        /// </returns>
        IObservable<int> EjecutarSpAsync(int idConfiguracion, IDictionary<string, object>? parameters = null);
    }
}