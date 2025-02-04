using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Services.Logging;

namespace SPOrchestratorAPI.Models.Repositories
{
    /// <summary>
    /// Repositorio base genérico para operaciones CRUD de manera reactiva.
    /// </summary>
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly DbSet<T> _dbSet;
        protected readonly ApplicationDbContext _context;
        protected readonly ILoggerService<RepositoryBase<T>> _logger;

        // Constructor con 2 parámetros: Contexto y Logger
        protected RepositoryBase(ApplicationDbContext context, ILoggerService<RepositoryBase<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene el contexto de la base de datos.
        /// </summary>
        protected ApplicationDbContext Context => _context;

        /// <summary>
        /// Obtiene todos los registros de la entidad de manera reactiva.
        /// </summary>
        public IObservable<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            return Observable.FromAsync(async () =>
            {
                try
                {
                    IQueryable<T> query = _dbSet;
                    if (filter != null)
                    {
                        query = query.Where(filter);
                    }

                    return await query.ToListAsync();
                }
                catch (Exception ex)
                {
                    // Log the error using the logger service
                    _logger.LogError($"Error en GetAllAsync: {ex.Message}", ex);
                    throw new Exception("Error al obtener los registros.");
                }
            });
        }

        /// <summary>
        /// Obtiene un registro por su clave primaria de manera reactiva.
        /// </summary>
        public IObservable<T?> GetByIdAsync<TKey>(TKey id) where TKey : notnull
        {
            return Observable.FromAsync(async () =>
            {
                try
                {
                    return await _dbSet.FindAsync(id);
                }
                catch (Exception ex)
                {
                    // Log the error using the logger service
                    _logger.LogError($"Error en GetByIdAsync con ID {id}: {ex.Message}", ex);
                    throw new Exception($"Error al obtener el registro con ID {id}.");
                }
            });
        }

        public IObservable<T> AddAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return Observable.Create<T>(observer =>
            {
                try
                {
                    // Agregar la entidad al DbSet
                    _dbSet.Add(entity);

                    // Guardar los cambios en la base de datos de manera asincrónica
                    _context.SaveChangesAsync().ContinueWith(task =>
                    {
                        if (task.Exception != null)
                        {
                            _logger.LogError($"Error en AddAsync: {task.Exception.Message}", task.Exception);
                            observer.OnError(new Exception("Error al agregar el registro."));
                        }
                        else
                        {
                            observer.OnNext(entity); // Emitir el objeto agregado
                            observer.OnCompleted(); // Completar la secuencia
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error en AddAsync: {ex.Message}", ex);
                    observer.OnError(new Exception("Error al agregar el registro."));
                }

                return Disposable.Empty; // Limpieza al finalizar
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
                try
                {
                    _dbSet.Update(entity);
                    await _context.SaveChangesAsync();
                    return Unit.Default;
                }
                catch (Exception ex)
                {
                    // Log the error using the logger service
                    _logger.LogError($"Error en UpdateAsync: {ex.Message}", ex);
                    throw new Exception("Error al actualizar el registro.");
                }
            });
        }

        /// <summary>
        /// Elimina un registro de la base de datos de manera reactiva.
        /// </summary>
        public IObservable<Unit> DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return Observable.FromAsync(async () =>
            {
                try
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                    return Unit.Default;
                }
                catch (Exception ex)
                {
                    // Log the error using the logger service
                    _logger.LogError($"Error en DeleteAsync: {ex.Message}", ex);
                    throw new Exception("Error al eliminar el registro.");
                }
            });
        }
    }
}