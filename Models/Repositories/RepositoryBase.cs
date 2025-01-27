using System.Linq.Expressions;
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
    /// Obtiene todos los registros de la entidad, con opción de aplicar un filtro.
    /// </summary>
    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtiene un registro por su clave primaria.
    /// </summary>
    public async Task<T?> GetByIdAsync<TKey>(TKey id) where TKey : notnull
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Agrega un nuevo registro a la base de datos.
    /// </summary>
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Actualiza un registro existente en la base de datos.
    /// </summary>
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un registro de la base de datos.
    /// </summary>
    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}