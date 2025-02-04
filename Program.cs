using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Services;
using SPOrchestratorAPI.Services.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configurar controladores
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddSwaggerConfiguration();

// Configurar base de datos 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Habilitar acceso al contexto HTTP
builder.Services.AddHttpContextAccessor();

// Registro de servicios
builder.Services.AddScoped<AuditEntitiesService>();

// Registro de ILoggerService para RepositoryBase<T>
builder.Services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));

// Repositorios genéricos y específicos
builder.Services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));
builder.Services.AddScoped<ILoggerService<ServicioRepository>, LoggerService<ServicioRepository>>();
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();
builder.Services.AddScoped<IRepository<ServicioConfiguracion>, ServicioConfiguracionRepository>();
builder.Services.AddScoped<ServicioRepository>();

// Servicios de negocio
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();

// Registra el IServiceExecutor y su implementación
builder.Services.AddScoped<IServiceExecutor, ReactiveServiceExecutor>();

// Agregar logs a consola y depuración
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// Llamar al Inicializador de la Base de Datos
DatabaseInitializer.Initialize(app.Services);

// Configurar Swagger solo en entornos de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();