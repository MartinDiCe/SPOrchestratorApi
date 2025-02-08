using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ParameterRepositories
{
    /// <summary>
    /// Define las operaciones de repositorio para la entidad <see cref="Parameter"/>.
    /// </summary>
    public interface IParameterRepository
    {
        /// <summary>
        /// Crea un nuevo parámetro en la base de datos.
        /// </summary>
        /// <param name="parameter">El objeto <see cref="Parameter"/> a crear.</param>
        /// <returns>Una tarea que, al completarse, devuelve el parámetro creado.</returns>
        Task<Parameter> CreateAsync(Parameter parameter);

        /// <summary>
        /// Obtiene un parámetro por su identificador único.
        /// </summary>
        /// <param name="parameterId">El identificador del parámetro.</param>
        /// <returns>
        /// Una tarea que, al completarse, devuelve el objeto <see cref="Parameter"/> si se encuentra;
        /// de lo contrario, devuelve <c>null</c>.
        /// </returns>
        Task<Parameter?> GetByIdAsync(int parameterId);

        /// <summary>
        /// Obtiene un parámetro por su nombre.
        /// </summary>
        /// <param name="parameterName">El nombre del parámetro.</param>
        /// <returns>
        /// Una tarea que, al completarse, devuelve el objeto <see cref="Parameter"/> si se encuentra;
        /// de lo contrario, devuelve <c>null</c>.
        /// </returns>
        Task<Parameter?> GetByNameAsync(string parameterName);
    }
}