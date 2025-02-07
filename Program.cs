using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Middleware;
using SPOrchestratorAPI.Models.Repositories.ServicioConfiguracionRepositories;
using SPOrchestratorAPI.Models.Repositories.ServicioRepositories;
using SPOrchestratorAPI.Services.AuditServices;
using SPOrchestratorAPI.Services.ConnectionTesting;
using SPOrchestratorAPI.Services.LoggingServices;
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

// Método de extensión que agrega y configura Swagger 
// (por ejemplo, AddEndpointsApiExplorer, AddSwaggerGen, etc.)
builder.Services.AddSwaggerConfiguration();

// Configurar respuesta personalizada para errores de model binding utilizando la clase auxiliar
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context => ModelValidationResponseFactory.CustomResponse(context.ModelState);
});

// ---------------------------------------------------------
// 2) Configurar base de datos y DbContext
// ---------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------
// 3) Habilitar acceso al contexto HTTP para inyectar IHttpContextAccessor
// ---------------------------------------------------------
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------
// 4) Registrar servicios y utilidades personalizadas
// ---------------------------------------------------------

// Registramos el "executor" reactivo para manejar acciones y excepciones
builder.Services.AddScoped<IServiceExecutor, ReactiveServiceExecutor>();

// Servicio que aplica auditoría a entidades derivadas de AuditEntities
builder.Services.AddScoped<AuditEntitiesService>();

// Logging genérico
builder.Services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));

// Repositorio y su logger específico (opcional, 
// si deseas logs con la categoría 'ServicioRepository' en particular)
builder.Services.AddScoped<ILoggerService<ServicioRepository>, LoggerService<ServicioRepository>>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioConfiguracionRepository, ServicioConfiguracionRepository>();

// Servicio de dominio para la entidad "Servicio"
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();
builder.Services.AddScoped<IConnectionTester, ConnectionTester>();
builder.Services.AddScoped<IServicioConfiguracionConnectionTestService, ServicioConfiguracionConnectionTestService>();
builder.Services.AddScoped<IStoredProcedureService, StoredProcedureService>();


// ---------------------------------------------------------
// 5) Configurar logging
// ---------------------------------------------------------
builder.Logging.ClearProviders();         // Quita los proveedores de log por defecto
builder.Logging.AddConsole();            // Agrega log en consola
builder.Logging.AddDebug();              // Agrega log en ventana de depuración
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Nivel mínimo de detalle

var app = builder.Build();

// ---------------------------------------------------------
// 6) Inicializar la base de datos (semillas, migraciones, etc.)
// ---------------------------------------------------------
DatabaseInitializer.Initialize(app.Services);

// ---------------------------------------------------------
// 7) Pipeline de Middlewares
// ---------------------------------------------------------

// Usar Swagger en entornos de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
}

// Capturar excepciones y comunicarlas con el controlador
app.UseMiddleware<ExceptionMiddleware>();

// Autorización (si tienes endpoints que la requieran)
app.UseAuthorization();

// Enruta las peticiones a los endpoints de Controllers
app.MapControllers();

// Arranca la aplicación
app.Run();
