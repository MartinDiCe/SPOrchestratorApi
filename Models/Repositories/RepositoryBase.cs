using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;

namespace SPOrchestratorAPI.Models.Repositories;

/// <summary>
/// Repositorio base genérico para operaciones CRUD de manera reactiva.
/// </summary>
public class RepositoryBase<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet;

    protected RepositoryBase(ApplicationDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = Context.Set<T>();
    }

    /// <summary>
    /// Obtiene el contexto de la base de datos.
    /// </summary>
    protected ApplicationDbContext Context { get; }

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
        })
        .Catch(Observable.Return(Enumerable.Empty<T>()));
    }

    /// <summary>
    /// Obtiene un registro por su clave primaria de manera reactiva.
    /// </summary>
    public IObservable<T?> GetByIdAsync<TKey>(TKey id) where TKey : notnull
    {
        return Observable.FromAsync(async () => await _dbSet.FindAsync(id))
        .Catch(Observable.Return<T?>(null));
    }

    /// <summary>
    /// Agrega un nuevo registro a la base de datos de manera reactiva.
    /// </summary>
    public IObservable<T> AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return Observable.FromAsync(async () =>
            {
                // Verifica si la entidad ya existe antes de insertarla
                var existingEntity = await _dbSet.FindAsync(entity);

                if (existingEntity != null)
                {
                    throw new InvalidOperationException("La entidad ya existe en la base de datos.");
                }

                await _dbSet.AddAsync(entity);
                await Context.SaveChangesAsync();
                return entity;
            })
            .Catch<T, Exception>(ex =>
            {
                Console.WriteLine($"Error en AddAsync: {ex.Message}");
                return Observable.Throw<T>(ex);
            });
    }
    
    /// <summary>
    /// Actualiza un registro existente en la base de datos de manera reactiva.
    /// </summary>
    public IObservable<Unit> UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return Observable.FromAsync(async () =>
        {
            _dbSet.Update(entity);
            await Context.SaveChangesAsync();
            return Unit.Default;
        })
        .Catch(Observable.Return(Unit.Default));
    }

    /// <summary>
    /// Elimina un registro de la base de datos de manera reactiva.
    /// </summary>
    public IObservable<Unit> DeleteAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return Observable.FromAsync(async () =>
        {
            _dbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return Unit.Default;
        })
        .Catch(Observable.Return(Unit.Default));
    }
}
