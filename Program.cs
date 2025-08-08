using System.Text.Json.Serialization;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
// 0) Registrar DatabaseConfig para inicialización
// ---------------------------------------------------------
builder.Services.AddSingleton<DatabaseConfig>();

// ---------------------------------------------------------
// 1) Configurar servicios básicos (Controllers, Swagger, EF, Hangfire)
// ---------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSwaggerConfiguration();
builder.Services.Configure<ApiBehaviorOptions>(opts =>
    opts.InvalidModelStateResponseFactory =
        context => ModelValidationResponseFactory.CustomResponse(context.ModelState)
);

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------
// 2) Registrar repositorios y servicios de aplicación
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
// 3) Logging condicional
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
    builder.Logging.AddFilter("Hangfire.Server.Worker", LogLevel.Debug);
    builder.Logging.AddFilter(
        "SPOrchestratorAPI.Services.SPOrchestratorServices.ScheduledOrchestratorService",
        LogLevel.Debug
    );
}

// ---------------------------------------------------------
// 4) Antes de Build: crear BD, migraciones y seed
// ---------------------------------------------------------
using (var tempProvider = builder.Services.BuildServiceProvider())
{
    DatabaseInitializer.Initialize(tempProvider);
}

// ---------------------------------------------------------
// 5) Antes de Build: habilitar New Relic y CORS dinámico
// ---------------------------------------------------------
builder.AddNewRelicIfEnabled();
await builder.AddDynamicCorsAsync();

// ---------------------------------------------------------
// 6) Build y configurar pipeline
// ---------------------------------------------------------
var app = builder.Build();

// 6.1) Hangfire
app.UseHangfireServer();
await HangfireJobsInitializer.CleanUnscheduledJobsAsync(
    app.Services,
    app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("HangfireJobsInitializer")
);
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IRecurringJobRegistrar>()
         .RegisterAllJobs();
}

// 6.2) Swagger y Hangfire Dashboard con toggles
app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/swagger"), branch =>
{
    branch.UseMiddleware<FeatureToggleMiddleware>("SwaggerEnabled");
    branch.UseSwagger();
    branch.UseSwaggerUI();
});
app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/hangfire"), branch =>
{
    branch.UseMiddleware<FeatureToggleMiddleware>("HangfireEnabled");

    var dashboardOptions = new DashboardOptions
    {
        Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
    };

    branch.UseHangfireDashboard(
        builder.Configuration.GetValue<string>("Hangfire:DashboardPath") ?? "/hangfire",
        dashboardOptions
    );
});

// 6.3) Middlewares finales
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}
app.UseDynamicCors();
app.UseMiddleware<ApiTraceMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
ApiTraceBus.StartTraceSubscriber(app.Services.GetRequiredService<IServiceScopeFactory>());

app.UseAuthorization();
app.MapControllers();
app.Run();