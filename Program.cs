using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar controladores
builder.Services.AddControllers();

// Configurar Swagger
builder.Services.AddSwaggerConfiguration();

// Configurar base de datos
builder.Services.AddSingleton<DatabaseConfig>();

// Habilitar acceso al contexto HTTP
builder.Services.AddHttpContextAccessor();

// Registro de servicios (revisado)
builder.Services.AddScoped<AuditEntitiesService>();

// Repositorios genéricos y específicos
builder.Services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>)); // Registro genérico
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();
builder.Services.AddScoped<IRepository<ServicioConfiguracion>, ServicioConfiguracionRepository>();

// Servicios de negocio
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IServicioConfiguracionService, ServicioConfiguracionService>();

// Construir y ejecutar la aplicación
var app = builder.Build();

// Configurar Swagger solo en entornos de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
