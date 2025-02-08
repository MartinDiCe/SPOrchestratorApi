using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Middleware;
using SPOrchestratorAPI.Models.Repositories.ApiTraceRepositories;
using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioRepositories;
using SPOrchestratorAPI.Services.ApiTraceServices;
using SPOrchestratorAPI.Services.AuditServices;
using SPOrchestratorAPI.Services.ConnectionTesting;
using SPOrchestratorAPI.Services.LoggingServices;
using SPOrchestratorAPI.Services.ParameterServices;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using SPOrchestratorAPI.Services.StoreProcedureServices;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1) Configurar servicios básicos (Controllers, Swagger, etc.)
// ---------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Método de extensión para agregar y configurar Swagger.
builder.Services.AddSwaggerConfiguration();

// Configurar respuesta personalizada para errores de model binding.
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
builder.Services.AddScoped<IParameterRepository, ParameterRepository>();
builder.Services.AddScoped<IApiTraceRepository, ApiTraceRepository>();

builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();
builder.Services.AddScoped<IConnectionTester, ConnectionTester>();
builder.Services.AddScoped<IServicioConfiguracionConnectionTestService, ServicioConfiguracionConnectionTestService>();
builder.Services.AddScoped<IStoredProcedureService, StoredProcedureService>();
builder.Services.AddScoped<IParameterService, ParameterService>();
builder.Services.AddScoped<IApiTraceService, ApiTraceService>();

// ---------------------------------------------------------
// 5) Configurar logging
// ---------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// ---------------------------------------------------------
// 6) Inicializar la base de datos (semillas, migraciones, etc.)
// ---------------------------------------------------------
DatabaseInitializer.Initialize(app.Services);

// ---------------------------------------------------------
// 7) Pipeline de Middlewares
// ---------------------------------------------------------

// En entornos de desarrollo, usar Swagger y un middleware de logging de request, response.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}

// Middleware que captura la traza de la API y la publica en el bus reactivo.
// Con este enfoque, el middleware no bloquea la respuesta al cliente.
app.UseMiddleware<ApiTraceMiddleware>();

// Middleware para captura global de excepciones.
app.UseMiddleware<ExceptionMiddleware>();

// Iniciar el suscriptor reactivo para las trazas.
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
SPOrchestratorAPI.Traces.ApiTraceBus.StartTraceSubscriber(scopeFactory);

// Middleware de autorización (si es necesario).
app.UseAuthorization();

// Enruta las peticiones a los endpoints de los Controllers.
app.MapControllers();

// Arranca la aplicación.
app.Run();
