using System.Reactive;
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;

namespace SPOrchestratorAPI.Services;

/// <summary>
/// Implementación del servicio para la gestión de configuraciones de servicios.
/// </summary>
public class ServicioConfiguracionService(IRepository<ServicioConfiguracion> servicioConfiguracionRepository)
    : IServicioConfiguracionService
{
    /// <inheritdoc />
    public IObservable<IEnumerable<ServicioConfiguracion>> GetAllAsync()
    {
        return servicioConfiguracionRepository.GetAllAsync(sc => !sc.Deleted)
            .Select(configs =>
            {
                var configList = configs.ToList();
                if (!configList.Any())
                {
                    throw new NotFoundException("No se encontraron configuraciones de servicios.");
                }
                return configList;
            });
    }

    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> GetByServicioIdAsync(int servicioId)
    {
        return servicioConfiguracionRepository.GetByIdAsync(servicioId)
            .Select(config => config ?? throw new NotFoundException($"No se encontró la configuración para el servicio con ID {servicioId}."));
    }

    /// <inheritdoc />
    public IObservable<ServicioConfiguracion> CreateAsync(ServicioConfiguracion config)
    {
        return Observable.FromAsync(async () =>
        {
            if (string.IsNullOrEmpty(config.NombreProcedimiento))
            {
                throw new ArgumentException("El nombre del procedimiento es obligatorio.");
            }
            return await servicioConfiguracionRepository.AddAsync(config);
        });
    }

    /// <inheritdoc />
    public IObservable<Unit> UpdateAsync(ServicioConfiguracion config)
    {
        return GetByServicioIdAsync(config.ServicioId)
            .SelectMany(_ =>
            {
                config.UpdatedAt = DateTime.UtcNow;
                config.UpdatedBy = "System";
                return servicioConfiguracionRepository.UpdateAsync(config);
            });
    }

    /// <inheritdoc />
    public IObservable<Unit> DeleteBySystemAsync(int id)
    {
        return GetByServicioIdAsync(id)
            .SelectMany(config =>
            {
                config.Deleted = true;
                config.DeletedAt = DateTime.UtcNow;
                config.DeletedBy = "System";
                return servicioConfiguracionRepository.UpdateAsync(config);
            });
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
                return servicioConfiguracionRepository.UpdateAsync(config);
            });
    }
}