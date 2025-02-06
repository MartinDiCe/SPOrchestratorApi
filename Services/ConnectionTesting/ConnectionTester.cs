using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Services.ConnectionTesting
{
    /// <summary>
    /// Implementa la lógica para probar la conexión a una base de datos según el proveedor especificado.
    /// Esta clase utiliza diferentes constructores de cadena de conexión dependiendo del proveedor.
    /// </summary>
    public class ConnectionTester : IConnectionTester
    {
        /// <inheritdoc />
        public async Task<ConnectionTestResult> TestConnectionAsync(string connectionString, DatabaseProvider provider)
        {
            var result = new ConnectionTestResult();

            try
            {
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
            }

            return result;
        }
    }
}

