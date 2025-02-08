namespace SPOrchestratorAPI.Services.RetriesService;

/// <summary>
/// Define la política de reintentos para la ejecución de una acción asíncrona.
/// Esta interfaz permite encapsular la lógica para reintentar una operación en caso de error,
/// aplicando un timeout individual por cada intento y un timeout global para la operación completa.
/// Además, se controla el número máximo de intentos permitidos.
/// </summary>
public interface IRetryPolicy
{
    
        /// <summary>
        /// Ejecuta una acción asíncrona aplicando la política de reintentos definida.
        /// La operación se intenta ejecutar hasta que se alcance el número máximo de intentos, 
        /// se supere el timeout global o la acción se ejecute con éxito.
        /// 
        /// Se utiliza un <see cref="CancellationToken"/> para aplicar un timeout por cada intento individual.
        /// Si ocurre un error transitorio (por ejemplo, un timeout o error de red), la política espera un tiempo de backoff antes de reintentar.
        /// </summary>
        /// <typeparam name="T">
        /// El tipo del resultado que se espera de la acción asíncrona.
        /// </typeparam>
        /// <param name="action">
        /// La acción asíncrona a ejecutar, la cual recibe un <see cref="CancellationToken"/> que debe utilizarse para cancelar
        /// la operación si se excede el timeout individual.
        /// </param>
        /// <param name="perAttemptTimeoutMs">
        /// El timeout en milisegundos para cada intento individual.
        /// Si la operación no se completa dentro de este tiempo, se cancela el intento y se considera fallido.
        /// </param>
        /// <param name="maxGlobalTimeoutMs">
        /// El timeout global en milisegundos para la operación completa, incluyendo todos los reintentos.
        /// Si el tiempo total transcurrido supera este valor, se aborta la operación.
        /// </param>
        /// <param name="maxAttempts">
        /// El número máximo de intentos a realizar. Si se alcanza este número sin éxito, se lanza una excepción.
        /// </param>
        /// <returns>
        /// Una tarea que, al completarse exitosamente, devuelve un resultado de tipo <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// La implementación de la política debe determinar si un error es transitorio y, en ese caso,
        /// reintentar la operación aplicando un retardo (por ejemplo, backoff exponencial) entre cada intento.
        /// Si el error no es transitorio, se lanza inmediatamente la excepción.
        /// Además, si los valores de timeout o de número máximo de intentos son 0 o nulos, se puede optar por
        /// ejecutar la operación una única vez sin aplicar reintentos.
        /// </remarks>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, int perAttemptTimeoutMs, int maxGlobalTimeoutMs, int maxAttempts);
}