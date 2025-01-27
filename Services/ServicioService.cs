using System;
using System.Reactive;
using System.Reactive.Linq;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;
using SPOrchestratorAPI.Exceptions;

namespace SPOrchestratorAPI.Services;

public class ServicioService
{
    private readonly IRepository<Servicio> _servicioRepository;

    public ServicioService(IRepository<Servicio> servicioRepository)
    {
        _servicioRepository = servicioRepository;
    }

    /// <summary>
    /// Obtiene todos los servicios activos (excluyendo eliminados) de manera reactiva.
    /// </summary>
    public IObservable<IEnumerable<Servicio>> GetAllAsync()
    {
        return _servicioRepository.GetAllAsync(s => !s.Deleted)
            .Select(servicios =>
            {
                if (!servicios.Any())
                {
                    throw new NotFoundException("No se encontraron servicios.");
                }
                return servicios;
            });
    }

    /// <summary>
    /// Obtiene un servicio por su ID de manera reactiva.
    /// </summary>
    public IObservable<Servicio?> GetByIdAsync(int id)
    {
        return _servicioRepository.GetByIdAsync(id)
            .Select(servicio =>
            {
                if (servicio == null || servicio.Deleted)
                {
                    throw new NotFoundException($"No se encontró el servicio con ID {id}.");
                }
                return servicio;
            });
    }

    /// <summary>
    /// Obtiene un servicio por su nombre de manera reactiva.
    /// </summary>
    public IObservable<Servicio?> GetByNameAsync(string name)
    {
        return _servicioRepository.GetAllAsync(s => s.Name == name && !s.Deleted)
            .Select(servicios => servicios.FirstOrDefault());
    }

    /// <summary>
    /// Crea un nuevo servicio de manera reactiva.
    /// </summary>
    public IObservable<Servicio> CreateAsync(Servicio servicio)
    {
        return Observable.FromAsync(async () =>
        {
            if (string.IsNullOrEmpty(servicio.Name))
            {
                throw new ArgumentException("El nombre del servicio es obligatorio.");
            }
            return await _servicioRepository.AddAsync(servicio);
        });
    }

    /// <summary>
    /// Actualiza un servicio existente de manera reactiva.
    /// </summary>
    public IObservable<Unit> UpdateAsync(Servicio servicio)
    {
        return GetByIdAsync(servicio.Id)
            .SelectMany(existingServicio =>
            {
                servicio.UpdatedAt = DateTime.UtcNow;
                servicio.UpdatedBy = "System";
                return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            });
    }

    /// <summary>
    /// Cambia el estado de un servicio de manera reactiva.
    /// </summary>
    public IObservable<Unit> ChangeStatusAsync(int id, bool newStatus)
    {
        return GetByIdAsync(id)
            .SelectMany(servicio =>
            {
                servicio.Status = newStatus;
                servicio.UpdatedAt = DateTime.UtcNow;
                servicio.UpdatedBy = "System";
                return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            });
    }

    /// <summary>
    /// Marca un servicio como eliminado (eliminación lógica) de manera reactiva.
    /// </summary>
    public IObservable<Unit> DeleteBySystemAsync(int id)
    {
        return GetByIdAsync(id)
            .SelectMany(servicio =>
            {
                servicio.Deleted = true;
                servicio.DeletedAt = DateTime.UtcNow;
                servicio.DeletedBy = "System";
                return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            });
    }

    /// <summary>
    /// Restaura un servicio eliminado de manera reactiva.
    /// </summary>
    public IObservable<Unit> RestoreBySystemAsync(int id)
    {
        return _servicioRepository.GetByIdAsync(id)
            .SelectMany(servicio =>
            {
                if (servicio == null || !servicio.Deleted)
                {
                    throw new NotFoundException($"No se encontró un servicio eliminado con ID {id}.");
                }

                servicio.Deleted = false;
                servicio.DeletedAt = null;
                servicio.DeletedBy = null;
                servicio.UpdatedAt = DateTime.UtcNow;
                servicio.UpdatedBy = "System";
                return _servicioRepository.UpdateAsync(servicio).Select(_ => Unit.Default);
            });
    }
}

