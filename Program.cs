using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Admin;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Examples;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Middleware;
using SPOrchestratorAPI.Models.Repositories.ApiTraceRepositories;
using SPOrchestratorAPI.Models.Repositories.ContinueWithRepositories;
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioProgramacionRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioRepositories;
using SPOrchestratorAPI.Services.ApiTraceServices;
using SPOrchestratorAPI.Services.AuditServices;
using SPOrchestratorAPI.Services.ChainOrchestratorServices;
using SPOrchestratorAPI.Services.ConnectionTestingServices;
using SPOrchestratorAPI.Services.ContinueWithServices;
using SPOrchestratorAPI.Services.EndpointServices;
using SPOrchestratorAPI.Services.HangFireServices;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ParameterServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioProgramacionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using SPOrchestratorAPI.Services.SPOrchestratorServices;
using SPOrchestratorAPI.Services.StoreProcedureServices;
using SPOrchestratorAPI.Services.VistasSqlServices;
using SPOrchestratorAPI.Traces;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 0) Configurar Hangfire (storage + serializers)
// ---------------------------------------------------------
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(
           builder.Configuration.GetConnectionString("DefaultConnection"),
           new SqlServerStorageOptions
           {
               CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
               SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
               QueuePollInterval = TimeSpan.Zero,
               UseRecommendedIsolationLevel = true,
               DisableGlobalLocks = true
           });
});

// ---------------------------------------------------------
// 1) Registrar el servidor de Hangfire como HostedService
//    (¡Solo una vez!)
// ---------------------------------------------------------
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
    options.Queues      = new[] { "default" };
});

// ---------------------------------------------------------
// 2) Servicios básicos (Controllers, Swagger, ModelValidation)
// ---------------------------------------------------------
builder.Services.AddControllers()
       .AddJsonOptions(opts =>
           opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSwaggerConfiguration();
builder.Services.Configure<ApiBehaviorOptions>(opts =>
    opts.InvalidModelStateResponseFactory =
        context => ModelValidationResponseFactory.CustomResponse(context.ModelState)
);

// ---------------------------------------------------------
// 3) EF Core DbContext
// ---------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ---------------------------------------------------------
// 4) IHttpContextAccessor
// ---------------------------------------------------------
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------
// 5) Repositorios y servicios de aplicación
// ---------------------------------------------------------
builder.Services.AddSingleton<IRecurringJobRegistrar, RecurringJobRegistrar>();
builder.Services.AddScoped<IHangfireJobService, HangfireJobService>();


builder.Services.AddScoped<IServiceExecutor, ReactiveServiceExecutor>();
builder.Services.AddScoped<AuditEntitiesService>();
builder.Services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));

builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioConfiguracionRepository, ServicioConfiguracionRepository>();
builder.Services.AddScoped<IServicioProgramacionRepository, ServicioProgramacionRepository>();
builder.Services.AddScoped<IParameterRepository, ParameterRepository>();
builder.Services.AddScoped<IApiTraceRepository, ApiTraceRepository>();
builder.Services.AddScoped<IServicioContinueWithRepository, ServicioContinueWithRepository>();

builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IContinuidadHelper, ContinuidadHelper>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();
builder.Services.AddScoped<IServicioProgramacionService, ServicioProgramacionService>();
builder.Services.AddScoped<IScheduledOrchestratorService, ScheduledOrchestratorService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IConnectionTesterService, ConnectionTesterService>();
builder.Services.AddScoped<IServicioConfiguracionConnectionTestService, ServicioConfiguracionConnectionTestService>();
builder.Services.AddScoped<IStoredProcedureExecutorFactory, StoredProcedureExecutorFactory>();
builder.Services.AddScoped<IStoredProcedureTestService, StoredProcedureTestService>();
builder.Services.AddScoped<IStoredProcedureService, StoredProcedureService>();
builder.Services.AddScoped<ISpOrchestratorService, SpOrchestratorService>();
builder.Services.AddScoped<IVistaSqlService, VistaSqlService>();
builder.Services.AddScoped<IParameterService, ParameterService>();
builder.Services.AddScoped<IApiTraceService, ApiTraceService>();
builder.Services.AddScoped<IServicioContinueWithService, ServicioContinueWithService>();
builder.Services.AddScoped<IChainOrchestratorService, ChainOrchestratorService>();
builder.Services.AddHttpClient<IEndpointService, EndpointService>();

builder.Services.AddSwaggerExamplesFromAssemblyOf<StoredProcedureExecutionRequestMultipleExamples>();
builder.Services.AddMemoryCache();

// ---------------------------------------------------------
// 6) Logging condicional (añadimos filtros para Hangfire)
// ---------------------------------------------------------
if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
}
else
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

    builder.Logging.AddFilter("Hangfire.Server.RecurringJobScheduler", LogLevel.Debug);
    builder.Logging.AddFilter("Hangfire.Server.Worker",           LogLevel.Debug);
    builder.Logging.AddFilter(
        "SPOrchestratorAPI.Services.SPOrchestratorServices.ScheduledOrchestratorService",
        LogLevel.Debug
    );
}

var app = builder.Build();

// ---------------------------------------------------------
// 7) Dashboard de Hangfire (UI)
// ---------------------------------------------------------
app.UseHangfireDashboard(
    pathMatch: "/hangfire");

// ---------------------------------------------------------
// 8) Limpiar / Registrar / refrescar todos los recurring jobs
// ---------------------------------------------------------
await HangfireJobsInitializer.CleanUnscheduledJobsAsync(
    app.Services,
    app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("HangfireJobsInitializer"));

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
         .GetRequiredService<IRecurringJobRegistrar>()
         .RegisterAllJobs();
}

// ---------------------------------------------------------
// 9) Inicializar BD (migraciones, seeds, etc.)
// ---------------------------------------------------------
DatabaseInitializer.Initialize(app.Services);

// ---------------------------------------------------------
// 10) Pipeline de Middlewares
// ---------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ApiTraceMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

// Suscripción al bus de trazas
ApiTraceBus.StartTraceSubscriber(app.Services.GetRequiredService<IServiceScopeFactory>());

app.UseAuthorization();
app.MapControllers();
app.Run();
