using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Examples;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Middleware;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories.ApiTraceRepositories;
using SPOrchestratorAPI.Models.Repositories.ContinueWithRepositories;
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioProgramacionRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioRepositories;
using SPOrchestratorAPI.Services.ApiTraceServices;
using SPOrchestratorAPI.Services.AuditServices;
using SPOrchestratorAPI.Services.ConnectionTestingServices;
using SPOrchestratorAPI.Services.ContinueWithServices;
using SPOrchestratorAPI.Services.EndpointServices;
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
// 1) Configurar servicios básicos (Controllers, Swagger, etc.)
// ---------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerConfiguration();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context => ModelValidationResponseFactory.CustomResponse(context.ModelState);
});

// ---------------------------------------------------------
// 2) Configurar base de datos y DbContext
// ---------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------
// 3) Habilitar acceso al contexto HTTP (IHttpContextAccessor)
// ---------------------------------------------------------
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------
// 4) Registrar servicios y utilidades personalizadas
// ---------------------------------------------------------
builder.Services.AddScoped<IServiceExecutor, ReactiveServiceExecutor>();
builder.Services.AddScoped<AuditEntitiesService>();
builder.Services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));
builder.Services.AddScoped<ILoggerService<ServicioRepository>, LoggerService<ServicioRepository>>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioConfiguracionRepository, ServicioConfiguracionRepository>();
builder.Services.AddScoped<IServicioProgramacionRepository, ServicioProgramacionRepository>();
builder.Services.AddScoped<IParameterRepository, ParameterRepository>();
builder.Services.AddScoped<IApiTraceRepository, ApiTraceRepository>();
builder.Services.AddScoped<IServicioContinueWithRepository, ServicioContinueWithRepository>();

builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();
builder.Services.AddScoped<IServicioProgramacionService, ServicioProgramacionService>();
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

builder.Services.AddHttpClient<IEndpointService, EndpointService>();

builder.Services.AddSwaggerExamplesFromAssemblyOf<StoredProcedureExecutionRequestMultipleExamples>();

builder.Services.AddMemoryCache();

// ---------------------------------------------------------
// 5) Configurar logging de forma condicional
// ---------------------------------------------------------
if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
}
else
{
    // En desarrollo, se agregan los proveedores para consola y debug, con un nivel detallado.
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

var app = builder.Build();

// ---------------------------------------------------------
// 6) Inicializar la base de datos (semillas, migraciones, etc.)
// ---------------------------------------------------------
DatabaseInitializer.Initialize(app.Services);

// ---------------------------------------------------------
// 7) Pipeline de Middlewares
// ---------------------------------------------------------

// En entornos de desarrollo, usar Swagger y el middleware de logging de request/response.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}

// Aquí decides si en producción quieres exponer Swagger o no
// app.UseSwaggerConfiguration(); // Si deseas exponer Swagger en producción, activa esta línea


// **IMPORTANTE:** Para capturar correctamente la respuesta (incluso en errores)
// es recomendable que el middleware de trazas (ApiTraceMiddleware) envuelva todo el pipeline,
// por lo que lo registramos **antes** de ExceptionMiddleware.
app.UseMiddleware<ApiTraceMiddleware>();

// Middleware para captura global de excepciones.
app.UseMiddleware<ExceptionMiddleware>();

// Iniciar el suscriptor reactivo para las trazas.
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
ApiTraceBus.StartTraceSubscriber(scopeFactory);

// Middleware de autorización (si es necesario).
app.UseAuthorization();

// Enruta las peticiones a los endpoints de los Controllers.
app.MapControllers();

// Arranca la aplicación.
app.Run();
