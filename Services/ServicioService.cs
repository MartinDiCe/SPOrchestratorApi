using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories;

namespace SPOrchestratorAPI.Services;

public class ServicioService
{
    private readonly IRepository<Servicio> _servicioRepository;

    public ServicioService(IRepository<Servicio> servicioRepository)
    {
        _servicioRepository = servicioRepository;
    }

    public async Task<IEnumerable<Servicio>> GetAllAsync() => await _servicioRepository.GetAllAsync();

    public async Task<Servicio?> GetByIdAsync(int id) => await _servicioRepository.GetByIdAsync(id);

    public async Task<Servicio> CreateAsync(Servicio servicio) => await _servicioRepository.AddAsync(servicio);

    public async Task UpdateAsync(Servicio servicio) => await _servicioRepository.UpdateAsync(servicio);

    public async Task DeleteAsync(Servicio servicio) => await _servicioRepository.DeleteAsync(servicio);
}