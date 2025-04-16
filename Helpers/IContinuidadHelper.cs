using SPOrchestratorAPI.Exceptions;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using System.Reactive.Linq; // Para poder usar FirstAsync/FirstOrDefaultAsync

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Interfaz para el helper que verifica si un servicio tiene configurada la continuidad.
    /// </summary>
    public interface IContinuidadHelper
    {
        /// <summary>
        /// Determina de forma asíncrona si el servicio identificado por <paramref name="serviceName"/> tiene configurada la continuidad.
        /// </summary>
        /// <param name="serviceName">El nombre del servicio a evaluar.</param>
        /// <returns>
        /// Un <see cref="Task{Boolean}"/> que devuelve <c>true</c> si la configuración del servicio indica que debe continuar, 
        /// o <c>false</c> en caso contrario.
        /// </returns>
        Task<bool> TieneContinuidadConfiguradaAsync(string serviceName);
    }

    /// <summary>
    /// Implementación del helper para validar la continuidad de un servicio.
    /// Se consulta el servicio y, a partir de su configuración, se determina si está activada la continuidad.
    /// </summary>
    public class ContinuidadHelper : IContinuidadHelper
    {
        private readonly IServicioService _servicioService;
        private readonly IServicioConfiguracionService _configService;

        /// <summary>
        /// Constructor que inyecta los servicios necesarios para evaluar la continuidad.
        /// </summary>
        /// <param name="servicioService">Servicio para obtener la información del servicio.</param>
        /// <param name="configService">Servicio para obtener la configuración del servicio.</param>
        /// <exception cref="ArgumentNullException">Si alguna de las dependencias es nula.</exception>
        public ContinuidadHelper(IServicioService servicioService, IServicioConfiguracionService configService)
        {
            _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <inheritdoc/>
        public async Task<bool> TieneContinuidadConfiguradaAsync(string serviceName)
        {
            // Buscar el servicio por su nombre
            var servicio = await _servicioService.GetByNameAsync(serviceName).FirstOrDefaultAsync();
            if (servicio == null)
            {
                throw new ResourceNotFoundException($"No se encontró un servicio con el nombre '{serviceName}'.");
            }

            // Obtener la configuración del servicio
            var configs = await _configService.GetByServicioIdAsync(servicio.Id).FirstAsync();
            if (configs == null || configs.Count == 0)
            {
                return false;
            }

            // Suponemos que la continuidad se define en la primera configuración disponible.
            return configs[0].ContinuarCon;
        }
    }
}
