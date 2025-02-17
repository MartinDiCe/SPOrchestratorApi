using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.DTOs.ServicioProgramacioDtos;
using SPOrchestratorAPI.Models.Entities;
using SPOrchestratorAPI.Models.Repositories.ServicioProgramacionRepositories;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.ServicioProgramacionServices
{
    /// <summary>
    /// Implementación de <see cref="IServicioProgramacionService"/> que contiene la lógica de negocio,
    /// incluida la validación del formato CRON.
    /// </summary>
    public class ServicioProgramacionService(
        IServicioProgramacionRepository repository,
        ILoggerService<ServicioProgramacionService> logger)
        : IServicioProgramacionService
    {
        private readonly IServicioProgramacionRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILoggerService<ServicioProgramacionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> CreateAsync(CreateServicioProgramacionDto dto)
        {
            if (!CronValidator.IsValid(dto.CronExpression))
            {
                throw new ArgumentException(
                    $"La expresión CRON es inválida. Ejemplo válido: \"5 17 * * *\". " +
                    $"Para más información, consulte https://crontab.guru/#*_*_*_*_*",
                    nameof(dto.CronExpression));
            }

            var programacion = new ServicioProgramacion
            {
                ServicioConfiguracionId = dto.ServicioConfiguracionId,
                CronExpression = dto.CronExpression,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ServicioConfiguracion = new ServicioConfiguracion
                {
                    Id = dto.ServicioConfiguracionId,
                    Servicio = new Servicio() 
                }
            };

            _logger.LogInfo("Iniciando la creación de una nueva programación de servicio...");
            return _repository.CreateAsync(programacion);
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> UpdateAsync(UpdateServicioProgramacionDto dto)
        {
            if (dto.Id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que 0.", nameof(dto.Id));
            }
            
            if (!CronValidator.IsValid(dto.CronExpression))
            {
                throw new ArgumentException(
                    $"La expresión CRON es inválida. Ejemplo válido: \"5 17 * * *\". " +
                    $"Para más información, consulte https://crontab.guru/#*_*_*_*_*",
                    nameof(dto.CronExpression));
            }

            var programacion = new ServicioProgramacion
            {
                ServicioConfiguracionId = dto.ServicioConfiguracionId,
                CronExpression = dto.CronExpression,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ServicioConfiguracion = new ServicioConfiguracion
                {
                    Id = dto.ServicioConfiguracionId,
                    Servicio = new Servicio() 
                }
            };

            _logger.LogInfo($"Iniciando la actualización de la programación con ID {dto.Id}...");
            return _repository.UpdateAsync(programacion);
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        /// <inheritdoc />
        public IObservable<IList<ServicioProgramacion>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        /// <inheritdoc />
        public IObservable<IList<ServicioProgramacion>> GetByServicioConfiguracionIdAsync(int servicioConfiguracionId)
        {
            return _repository.GetByServicioConfiguracionIdAsync(servicioConfiguracionId);
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> SoftDeleteAsync(int id)
        {
            return _repository.SoftDeleteAsync(id);
        }

        /// <inheritdoc />
        public IObservable<ServicioProgramacion> RestoreAsync(int id)
        {
            return _repository.RestoreAsync(id);
        }
    }
}
