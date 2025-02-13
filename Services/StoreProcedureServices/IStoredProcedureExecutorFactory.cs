using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Services.LoggingServices;

namespace SPOrchestratorAPI.Services.StoreProcedureServices
{
    /// <summary>
    /// Factoría para obtener la implementación de <see cref="IStoredProcedureExecutor"/> 
    /// según el proveedor de base de datos.
    /// </summary>
    public interface IStoredProcedureExecutorFactory
    {
        /// <summary>
        /// Retorna una instancia de <see cref="IStoredProcedureExecutor"/> adecuada para el proveedor especificado.
        /// </summary>
        /// <param name="provider">
        /// El proveedor de base de datos para el cual se requiere el executor (por ejemplo, <see cref="DatabaseProvider.SqlServer"/>).
        /// </param>
        /// <returns>
        /// Una instancia de <see cref="IStoredProcedureExecutor"/> que permite ejecutar stored procedures para el proveedor indicado.
        /// </returns>
        IStoredProcedureExecutor GetExecutor(DatabaseProvider provider);
    }

    /// <summary>
    /// Implementación de <see cref="IStoredProcedureExecutorFactory"/> que retorna la implementación adecuada 
    /// de <see cref="IStoredProcedureExecutor"/> basándose en el <see cref="DatabaseProvider"/>.
    /// </summary>
    /// <param name="sqlLogger">
    /// Servicio de logging para la implementación de <see cref="SqlServerStoredProcedureExecutor"/>.
    /// </param>
    public class StoredProcedureExecutorFactory(ILoggerService<SqlServerStoredProcedureExecutor> sqlLogger)
        : IStoredProcedureExecutorFactory
    {
        /// <inheritdoc />
        public IStoredProcedureExecutor GetExecutor(DatabaseProvider provider)
        {
            return provider switch
            {
                DatabaseProvider.SqlServer => new SqlServerStoredProcedureExecutor(sqlLogger),
                _ => throw new NotSupportedException("Proveedor de base de datos no soportado para ejecución de SP.")
            };
        }
    }
}