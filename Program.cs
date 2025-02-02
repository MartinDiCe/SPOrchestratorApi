using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar controladores
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddSwaggerConfiguration();

// Configurar base de datos (🔹 Agregar esto)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Habilitar acceso al contexto HTTP
builder.Services.AddHttpContextAccessor();

// Registro de servicios
builder.Services.AddScoped<AuditEntitiesService>();

// Repositorios genéricos y específicos
builder.Services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>)); 
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();
builder.Services.AddScoped<IRepository<ServicioConfiguracion>, ServicioConfiguracionRepository>();

// Servicios de negocio
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();

// Agregar logs a consola y depuración
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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