using System.Reactive; 
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Models.DTOs;


namespace SPOrchestratorAPI.Services;

/// <summary>
/// Implementación del servicio para la gestión de configuraciones de servicios.
/// </summary>
public class ServicioConfiguracionService(IRepository<ServicioConfiguracion> servicioConfiguracionRepository, 
    IRepository<Servicio> servicioRepository)
    : IServicioConfiguracionService
{
    /// <inheritdoc />
    public IObservable<IEnumerable<ServicioConfiguracionDtoResponse>> GetAllAsync()
    {
        return servicioConfiguracionRepository.GetAllAsync(sc => !sc.Deleted)
            .Select(configs =>
            {
                var configList = configs.ToList();
                if (!configList.Any())
                {
                    throw new NotFoundException("No se encontraron configuraciones de servicios.");
                }
                return configList.Select(config => new ServicioConfiguracionDtoResponse
                {
                    Id = config.Id,
                    ServicioId = config.ServicioId,
                    NombreProcedimiento = config.NombreProcedimiento,
                    ConexionBaseDatos = config.ConexionBaseDatos,
                    Parametros = config.Parametros ?? string.Empty,
                    MaxReintentos = config.MaxReintentos,
                    TimeoutSegundos = config.TimeoutSegundos,
                    CreatedAt = config.CreatedAt,
                    CreatedBy = config.CreatedBy,
                    UpdatedAt = config.UpdatedAt,
                    UpdatedBy = config.UpdatedBy ?? "Desconocido",
                    Deleted = config.Deleted,
                    DeletedAt = config.DeletedAt,
                    DeletedBy = config.DeletedBy ?? "Desconocido"
                }).ToList();
            })
            .Catch<IEnumerable<ServicioConfiguracionDtoResponse>, Exception>(ex => Observable.Throw<IEnumerable<ServicioConfiguracionDtoResponse>>(ex));
    }

    /// <inheritdoc />
    public IObservable<ServicioConfiguracionDtoResponse> GetByServicioIdAsync(int servicioId)
    {
        return servicioConfiguracionRepository.GetByIdAsync(servicioId)
            .Select(config =>
            {
                if (config == null)
                {
                    throw new NotFoundException($"No se encontró la configuración para el servicio con ID {servicioId}.");
                }
                return new ServicioConfiguracionDtoResponse
                {
                    Id = config.Id,
                    ServicioId = config.ServicioId,
                    NombreProcedimiento = config.NombreProcedimiento,
                    ConexionBaseDatos = config.ConexionBaseDatos,
                    Parametros = config.Parametros ?? string.Empty,
                    MaxReintentos = config.MaxReintentos,
                    TimeoutSegundos = config.TimeoutSegundos,
                    CreatedAt = config.CreatedAt,
                    CreatedBy = config.CreatedBy,
                    UpdatedAt = config.UpdatedAt,
                    UpdatedBy = config.UpdatedBy ?? "Desconocido",
                    Deleted = config.Deleted,
                    DeletedAt = config.DeletedAt,
                    DeletedBy = config.DeletedBy ?? "Desconocido"
                };
            })
            .Catch<ServicioConfiguracionDtoResponse, Exception>(ex => Observable.Throw<ServicioConfiguracionDtoResponse>(ex));
    }

    /// <inheritdoc />
    public IObservable<ServicioConfiguracionDtoResponse> CreateAsync(CreateServicioConfiguracionDto configDto)
    {
        return Observable.FromAsync(async () =>
            {
                if (string.IsNullOrEmpty(configDto.NombreProcedimiento))
                {
                    throw new ArgumentException("El nombre del procedimiento es obligatorio.");
                }

                var servicio = await servicioRepository.GetByIdAsync(configDto.ServicioId)
                    .FirstOrDefaultAsync(); 

                if (servicio == null)
                {
                    throw new NotFoundException($"No se encontró el servicio con ID {configDto.ServicioId}.");
                }

                var config = new ServicioConfiguracion
                {
                    ServicioId = configDto.ServicioId,
                    NombreProcedimiento = configDto.NombreProcedimiento,
                    ConexionBaseDatos = configDto.ConexionBaseDatos,
                    Parametros = configDto.Parametros,
                    MaxReintentos = configDto.MaxReintentos,
                    TimeoutSegundos = configDto.TimeoutSegundos,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Servicio = servicio // 🔥 Asigna el servicio antes de guardar
                };

                var createdConfig = await servicioConfiguracionRepository.AddAsync(config);

                return new ServicioConfiguracionDtoResponse
                {
                    Id = createdConfig.Id,
                    ServicioId = createdConfig.ServicioId,
                    NombreProcedimiento = createdConfig.NombreProcedimiento,
                    ConexionBaseDatos = createdConfig.ConexionBaseDatos,
                    Parametros = createdConfig.Parametros ?? string.Empty,
                    MaxReintentos = createdConfig.MaxReintentos,
                    TimeoutSegundos = createdConfig.TimeoutSegundos,
                    CreatedAt = createdConfig.CreatedAt,
                    CreatedBy = createdConfig.CreatedBy
                };
        })
        .Catch<ServicioConfiguracionDtoResponse, Exception>(ex => Observable.Throw<ServicioConfiguracionDtoResponse>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> UpdateAsync(UpdateServicioConfiguracionDto configDto)
    {
        return servicioConfiguracionRepository.GetByIdAsync(configDto.ServicioId)
            .SelectMany(config =>
            {
                if (config == null)
                {
                    throw new NotFoundException($"No se encontró la configuración para el servicio con ID {configDto.ServicioId}.");
                }
                config.NombreProcedimiento = configDto.NombreProcedimiento;
                config.ConexionBaseDatos = configDto.ConexionBaseDatos;
                config.Parametros = configDto.Parametros;
                config.MaxReintentos = configDto.MaxReintentos;
                config.TimeoutSegundos = configDto.TimeoutSegundos;
                config.UpdatedAt = DateTime.UtcNow;
                config.UpdatedBy = "System";
                return servicioConfiguracionRepository.UpdateAsync(config).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> DeleteBySystemAsync(int id)
    {
        return servicioConfiguracionRepository.GetByIdAsync(id)
            .SelectMany(config =>
            {
                if (config == null)
                {
                    throw new NotFoundException($"No se encontró la configuración para el servicio con ID {id}.");
                }
                config.Deleted = true;
                config.DeletedAt = DateTime.UtcNow;
                config.DeletedBy = "System";
                return servicioConfiguracionRepository.UpdateAsync(config).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> RestoreBySystemAsync(int id)
    {
        return servicioConfiguracionRepository.GetByIdAsync(id)
            .Select(config => config ?? throw new NotFoundException($"No se encontró una configuración eliminada con ID {id}."))
            .Where(config => config.Deleted)
            .SelectMany(config =>
            {
                config.Deleted = false;
                config.DeletedAt = null;
                config.DeletedBy = null;
                config.UpdatedAt = DateTime.UtcNow;
                config.UpdatedBy = "System";
                return servicioConfiguracionRepository.UpdateAsync(config).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }
}
