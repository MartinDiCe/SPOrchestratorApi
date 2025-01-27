using Microsoft.EntityFrameworkCore; 
using SPOrchestratorAPI.Configuration;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Services;
using SPOrchestratorAPI.Middleware;


var builder = WebApplication.CreateBuilder(args);
var dbConfig = new DatabaseConfig(builder.Configuration);
var connectionString = dbConfig.GetConnectionString();

builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddSingleton<DatabaseConfig>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditEntitiesService>();
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();
builder.Services.AddScoped<ServicioService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddTransient<StoredProcedureService>();

var app = builder.Build();

DatabaseInitializer.Initialize(app.Services);

app.UseSwaggerConfiguration();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();