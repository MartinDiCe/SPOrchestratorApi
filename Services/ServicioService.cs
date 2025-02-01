using System.Reactive;
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;

namespace SPOrchestratorAPI.Services;

/// <summary>
/// Implementación del servicio para la gestión de servicios.
/// </summary>
public class ServicioService(IRepository<Servicio> servicioRepository) : IServicioService
{
    /// <inheritdoc />
    public IObservable<IEnumerable<Servicio>> GetAllAsync()
    {
        return servicioRepository.GetAllAsync(s => !s.Deleted)
            .Select(servicios =>
            {
                var servicioList = servicios.ToList();
                if (!servicioList.Any())
                {
                    throw new NotFoundException("No se encontraron servicios.");
                }
                return servicioList;
            })
            .Catch<IEnumerable<Servicio>, Exception>(ex => Observable.Throw<IEnumerable<Servicio>>(ex));
    }

    /// <inheritdoc />
    public IObservable<Servicio> GetByIdAsync(int id)
    {
        return servicioRepository.GetByIdAsync(id)
            .Select(servicio => servicio ?? throw new NotFoundException($"No se encontró el servicio con ID {id}."))
            .Where(servicio => !servicio.Deleted)
            .Catch<Servicio, Exception>(ex => Observable.Throw<Servicio>(ex));
    }

    /// <inheritdoc />
    public IObservable<Servicio> GetByNameAsync(string name)
    {
        return servicioRepository.GetAllAsync(s => s.Name == name && !s.Deleted)
            .Select(servicios => servicios.FirstOrDefault() ?? throw new NotFoundException($"No se encontró el servicio con nombre {name}."))
            .Catch<Servicio, Exception>(ex => Observable.Throw<Servicio>(ex));
    }

    /// <inheritdoc />
    public IObservable<Servicio> CreateAsync(Servicio servicio)
    {
        return Observable.FromAsync(async () =>
        {
            if (string.IsNullOrEmpty(servicio.Name))
            {
                throw new ArgumentException("El nombre del servicio es obligatorio.");
            }
            return await servicioRepository.AddAsync(servicio);
        })
        .Catch<Servicio, Exception>(ex => Observable.Throw<Servicio>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> UpdateAsync(Servicio servicio)
    {
        return GetByIdAsync(servicio.Id)
            .SelectMany(_ =>
            {
                servicio.UpdatedAt = DateTime.UtcNow;
                servicio.UpdatedBy = "System";
                return servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> ChangeStatusAsync(int id, bool newStatus)
    {
        return GetByIdAsync(id)
            .SelectMany(servicio =>
            {
                servicio.Status = newStatus;
                servicio.UpdatedAt = DateTime.UtcNow;
                servicio.UpdatedBy = "System";
                return servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> DeleteBySystemAsync(int id)
    {
        return GetByIdAsync(id)
            .SelectMany(servicio =>
            {
                servicio.Deleted = true;
                servicio.DeletedAt = DateTime.UtcNow;
                servicio.DeletedBy = "System";
                return servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }

    /// <inheritdoc />
    public IObservable<Unit> RestoreBySystemAsync(int id)
    {
        return servicioRepository.GetByIdAsync(id)
            .Select(servicio => servicio ?? throw new NotFoundException($"No se encontró un servicio eliminado con ID {id}."))
            .Where(servicio => servicio.Deleted)
            .SelectMany(servicio =>
            {
                servicio.Deleted = false;
                servicio.DeletedAt = null;
                servicio.DeletedBy = null;
                servicio.UpdatedAt = DateTime.UtcNow;
                servicio.UpdatedBy = "System";
                return servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            })
            .Catch<Unit, Exception>(ex => Observable.Throw<Unit>(ex));
    }
}
