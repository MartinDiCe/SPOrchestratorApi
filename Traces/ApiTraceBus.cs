using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Services.ApiTraceServices;

namespace SPOrchestratorAPI.Traces
{
    /// <summary>
    /// Bus de eventos para la traza de la API. Los ApiTrace que se publiquen serán escritos en la base de datos de forma asíncrona.
    /// </summary>
    public static class ApiTraceBus
    {
        public static ISubject<ApiTrace> TraceSubject { get; } = new ReplaySubject<ApiTrace>(bufferSize: 100);

        /// <summary>
        /// Inicia el suscriptor que procesa y persiste las trazas.
        /// Cada traza se procesa en un nuevo scope para evitar usar un contexto ya eliminado.
        /// </summary>
        /// <param name="scopeFactory">La factoría de scopes del contenedor de dependencias.</param>
        public static void StartTraceSubscriber(IServiceScopeFactory scopeFactory)
        {
            TraceSubject
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(async trace =>
                {
                    try
                    {
                        // Crear un nuevo scope para cada traza
                        using (var scope = scopeFactory.CreateScope())
                        {
                            var traceService = scope.ServiceProvider.GetRequiredService<IApiTraceService>();
                            await traceService.CreateAsync(trace);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Registrar el error (puedes usar un logger en lugar de Console.Error)
                        Console.Error.WriteLine($"Error al guardar traza: {ex.Message}");
                    }
                });
        }
    }
}