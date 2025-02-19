using Hangfire;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Clase encargada de registrar los Recurring Jobs de Hangfire
    /// usando un enfoque 100% reactivo (sin bloqueo).
    /// </summary>
    public class HangfireJobsInitializer
    {
        /// <summary>
        /// Registra la suscripción reactiva que, al obtener las programaciones,
        /// crea los Recurring Jobs en Hangfire.
        /// </summary>
        /// <param name="app">Referencia a la aplicación para obtener servicios por DI.</param>
        public static void RegisterRecurringJobsReactively(WebApplication app)
        {
            // Creamos un scope para inyectar nuestros servicios
            using var scope = app.Services.CreateScope();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<HangfireJobsInitializer>>();
            var programacionService = scope.ServiceProvider.GetRequiredService<IServicioProgramacionService>();
            var scheduledOrchestrator  = scope.ServiceProvider.GetRequiredService<IScheduledOrchestratorService>();

            // Nos suscribimos de forma 100% reactiva a la obtención de programaciones
            programacionService.GetAllAsync()
                .Subscribe(
                    programaciones =>
                    {
                        if (programaciones == null || programaciones.Count == 0)
                        {
                            logger.LogInformation("No se encontraron programaciones en la base de datos.");
                            return;
                        }

                        foreach (var prog in programaciones)
                        {
                            var config = prog.ServicioConfiguracion;
                            if (!config.EsProgramado)
                            {
                                logger.LogInformation(
                                    "El ServicioConfiguracionId={ConfigId} no está marcado como programado, se ignora.",
                                    config.Id
                                );
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(prog.CronExpression))
                            {
                                logger.LogError(
                                    "ServicioProgramacionId={ProgId} no tiene CRON expression. Se omite la programación.",
                                    prog.Id
                                );
                                continue;
                            }

                            // Si deseas verificar EndDate (para no crear un job si ya expiró)
                            if (prog.EndDate != default && prog.EndDate < DateTime.UtcNow)
                            {
                                logger.LogInformation(
                                    "La programación {ProgId} expiró en {EndDate}. No se registrará en Hangfire.",
                                    prog.Id, prog.EndDate
                                );
                                continue;
                            }

                            // Creamos un ID único para el job
                            var jobId = $"SC-{config.Id}";

                            // Registramos un Recurring Job en Hangfire
                            RecurringJob.AddOrUpdate(
                                recurringJobId: jobId,
                                methodCall: () => scheduledOrchestrator.EjecutarProgramado(config.Id),
                                cronExpression: prog.CronExpression
                            );

                            logger.LogInformation(
                                "Se ha registrado RecurringJob='{JobId}' para ConfigId={ConfigId} con CRON='{Cron}'.",
                                jobId, config.Id, prog.CronExpression
                            );
                        }
                    },
                    ex =>
                    {
                        // onError: si ocurrió un error al obtener la lista
                        logger.LogError(ex, "Error al obtener las programaciones para Hangfire.");
                    },
                    () =>
                    {
                        // onCompleted: si el observable de programaciones se completa
                        logger.LogInformation("Suscripción de programaciones finalizada (reactiva).");
                    }
                );
        }
    }
}
