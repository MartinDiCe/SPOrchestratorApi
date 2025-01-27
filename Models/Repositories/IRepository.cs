using System.Linq.Expressions;

namespace SPOrchestratorAPI.Models.Repositories;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene todos los registros de la entidad, con opción de aplicar un filtro.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);

    /// <summary>
    /// Obtiene un registro por su clave primaria.
    /// </summary>
    Task<T?> GetByIdAsync<TKey>(TKey id) where TKey : notnull;

    /// <summary>
    /// Agrega un nuevo registro de servicio a la base de datos.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Actualiza un registro existente en la base de datos.
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Elimina un registro de la base de datos.
    /// </summary>
    Task DeleteAsync(T entity);
}