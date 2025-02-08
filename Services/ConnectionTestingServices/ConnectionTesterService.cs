using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using SPOrchestratorAPI.Models.DTOs.ConnectionDtos;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Services.ConnectionTestingServices
{
    /// <summary>
    /// Implementa la lógica para probar la conexión a una base de datos según el proveedor especificado.
    /// Esta clase utiliza diferentes constructores de cadena de conexión dependiendo del proveedor.
    /// </summary>
    public class ConnectionTesterService : IConnectionTesterService
    {
        private readonly ILogger<ConnectionTesterService> _logger;

        /// <summary>
        /// Crea una nueva instancia de <see cref="ConnectionTesterService"/> inyectando el logger.
        /// </summary>
        /// <param name="logger">Logger para registrar mensajes de información y error.</param>
        public ConnectionTesterService(ILogger<ConnectionTesterService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ConnectionTestResultDto> TestConnectionAsync(string connectionString, DatabaseProvider provider)
        {
            var result = new ConnectionTestResultDto();

            try
            {
                _logger.LogInformation("Probando conexión. Proveedor: {Provider}, Cadena de conexión: {ConnectionString}", provider, connectionString);

                switch (provider)
                {
                    case DatabaseProvider.SqlServer:
                        using (var connection = new SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            result.IsSuccess = true;
                            result.Message = "Conexión a SQL Server exitosa.";
                        }
                        break;

                    case DatabaseProvider.MySql:
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            result.IsSuccess = true;
                            result.Message = "Conexión a MySQL exitosa.";
                        }
                        break;

                    case DatabaseProvider.PostgreSql:
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            result.IsSuccess = true;
                            result.Message = "Conexión a PostgreSQL exitosa.";
                        }
                        break;

                    case DatabaseProvider.Oracle:
                        using (var connection = new OracleConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            result.IsSuccess = true;
                            result.Message = "Conexión a Oracle exitosa.";
                        }
                        break;

                    default:
                        result.IsSuccess = false;
                        result.Message = "Proveedor de base de datos no soportado.";
                        break;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "La conexión falló.";
                result.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error al probar la conexión.");
            }
            
            _logger.LogInformation("Resultado del test de conexión: {Result}", result);
            return result;
        }
    }
}