using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Middleware;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Services;
using SPOrchestratorAPI.Services.Logging;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1) Configurar servicios básicos (Controllers, Swagger, etc.)
// ---------------------------------------------------------
builder.Services.AddControllers();

// Método de extensión que agrega y configura Swagger 
// (por ejemplo, AddEndpointsApiExplorer, AddSwaggerGen, etc.)
builder.Services.AddSwaggerConfiguration();

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

// Servicio de dominio para la entidad "Servicio"
builder.Services.AddScoped<IServicioService, ServicioService>();

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
