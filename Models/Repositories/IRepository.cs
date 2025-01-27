using System.Linq.Expressions;
using System.Reactive;

namespace SPOrchestratorAPI.Models.Repositories;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene todos los registros de la entidad de manera reactiva, con opción de aplicar un filtro.
    /// </summary>
    IObservable<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);

    /// <summary>
    /// Obtiene un registro por su clave primaria de manera reactiva.
    /// </summary>
    IObservable<T?> GetByIdAsync<TKey>(TKey id) where TKey : notnull;

    /// <summary>
    /// Agrega un nuevo registro de servicio a la base de datos de manera reactiva.
    /// </summary>
    IObservable<T> AddAsync(T entity);

    /// <summary>
    /// Actualiza un registro existente en la base de datos de manera reactiva.
    /// </summary>
    IObservable<Unit> UpdateAsync(T entity);

    /// <summary>
    /// Elimina un registro de la base de datos de manera reactiva.
    /// </summary>
    IObservable<Unit> DeleteAsync(T entity);
}