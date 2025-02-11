using System.Reactive.Linq;
using Swashbuckle.AspNetCore.Filters;
using SPOrchestratorAPI.Models.DTOs.StoreProcedureDtos;
using SPOrchestratorAPI.Services.ServicioConfiguracionServices;
using SPOrchestratorAPI.Services.ServicioServices;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace SPOrchestratorAPI.Examples
{ 
    //NO IMPLEMENTARE AHORA ESTO, QUEDA EJEMPLO PARA REVISAR DESPUÉS
    /// <summary>
    /// Proveedor de ejemplos dinámicos para <see cref="StoredProcedureExecutionRequest"/>
    /// basado en la configuración de todos los servicios. Genera y cachea los ejemplos; 
    /// si la configuración no ha cambiado, se utiliza la versión cacheada.
    /// </summary>
    public class StoredProcedureExecutionRequestMultipleExamples : IExamplesProvider<Dictionary<string, StoredProcedureExecutionRequest>>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "StoredProcedureExecutionRequestExamples";

        /// <summary>
        /// Inyecta el IServiceProvider (para resolver servicios scoped en tiempo de ejecución) y el IMemoryCache.
        /// </summary>
        /// <param name="serviceProvider">El contenedor de servicios.</param>
        /// <param name="cache">El servicio de caché para almacenar los ejemplos.</param>
        public StoredProcedureExecutionRequestMultipleExamples(IServiceProvider serviceProvider, IMemoryCache cache)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        /// <summary>
        /// Retorna el diccionario de ejemplos para StoredProcedureExecutionRequest.
        /// Se utiliza el caché y se recalcula únicamente si la "huella" de la configuración ha cambiado.
        /// </summary>
        public Dictionary<string, StoredProcedureExecutionRequest> GetExamples()
        {
            if (_cache.TryGetValue(CacheKey, out CachedExamples cachedExamples))
            {
                var currentHash = BuildConfigurationHashAsync().GetAwaiter().GetResult();
                if (currentHash == cachedExamples.Hash)
                {
                    return cachedExamples.Examples;
                }
            }

            var examples = BuildExamplesAsync().GetAwaiter().GetResult();
            var hash = BuildConfigurationHashAsync().GetAwaiter().GetResult();

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            };

            _cache.Set(CacheKey, new CachedExamples { Examples = examples, Hash = hash }, cacheEntryOptions);
            return examples;
        }

        /// <summary>
        /// Recorre todos los servicios y genera un ejemplo para cada uno.
        /// </summary>
        private async Task<Dictionary<string, StoredProcedureExecutionRequest>> BuildExamplesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var servicioService = scope.ServiceProvider.GetRequiredService<IServicioService>();
            var configService = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();

            var serviceList = await servicioService.GetAllAsync();
            var examples = new Dictionary<string, StoredProcedureExecutionRequest>();

            foreach (var svc in serviceList)
            {
                // Se obtiene la lista de configuraciones para el servicio y se selecciona el primero (o se puede ajustar la lógica)
                var configList = await configService.GetByServicioIdAsync(svc.Id);
                var config = configList?.FirstOrDefault();
                // Creamos el diccionario de parámetros con el tipo correcto.
                var parameters = new Dictionary<string, object>();

                if (config != null && !string.IsNullOrWhiteSpace(config.Parametros))
                {
                    // Se asume que los parámetros están separados por ';'
                    var parts = config.Parametros.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        parameters[part.Trim()] = "";
                    }
                }

                examples.Add(svc.Name, new StoredProcedureExecutionRequest
                {
                    ServiceName = svc.Name,
                    Parameters = parameters,
                    IsFile = false
                });
            }

            return examples;
        }

        /// <summary>
        /// Calcula una "huella" (hash MD5) de la configuración de todos los servicios.
        /// Esto permite detectar cambios en la configuración.
        /// </summary>
        private async Task<string> BuildConfigurationHashAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var servicioService = scope.ServiceProvider.GetRequiredService<IServicioService>();
            var configService = scope.ServiceProvider.GetRequiredService<IServicioConfiguracionService>();

            var serviceList = await servicioService.GetAllAsync();
            var sb = new StringBuilder();

            foreach (var svc in serviceList.OrderBy(s => s.Id))
            {
                sb.Append(svc.Name);
                var configList = await configService.GetByServicioIdAsync(svc.Id);
                var config = configList?.FirstOrDefault();
                if (config != null && !string.IsNullOrEmpty(config.Parametros))
                {
                    sb.Append(config.Parametros);
                }
            }

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Clase auxiliar para almacenar los ejemplos y la huella de la configuración.
        /// </summary>
        private class CachedExamples
        {
            public Dictionary<string, StoredProcedureExecutionRequest> Examples { get; set; } = new();
            public string Hash { get; set; } = string.Empty;
        }
    }
}
