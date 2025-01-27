using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;

namespace SPOrchestratorAPI.Models.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public RepositoryBase(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Obtiene todos los registros de la entidad de manera reactiva.
    /// </summary>
    public IObservable<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        return Observable.FromAsync(async () =>
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        });
    }

    /// <summary>
    /// Obtiene un registro por su clave primaria de manera reactiva.
    /// </summary>
    public IObservable<T?> GetByIdAsync<TKey>(TKey id) where TKey : notnull
    {
        return Observable.FromAsync(async () => await _dbSet.FindAsync(id));
    }

    /// <summary>
    /// Agrega un nuevo registro a la base de datos de manera reactiva.
    /// </summary>
    public IObservable<T> AddAsync(T entity)
    {
        return Observable.FromAsync(async () =>
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        });
    }

    /// <summary>
    /// Actualiza un registro existente en la base de datos de manera reactiva.
    /// </summary>
    public IObservable<Unit> UpdateAsync(T entity)
    {
        return Observable.FromAsync(async () =>
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return Unit.Default;
        });
    }

    /// <summary>
    /// Elimina un registro de la base de datos de manera reactiva.
    /// </summary>
    public IObservable<Unit> DeleteAsync(T entity)
    {
        return Observable.FromAsync(async () =>
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return Unit.Default;
        });
    }
}
