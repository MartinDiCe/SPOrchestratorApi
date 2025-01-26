using Microsoft.Data.SqlClient;
using SPOrchestratorAPI.Configuration;

namespace SPOrchestratorAPI.Services;

public class StoredProcedureService
{
    private readonly string _connectionString;

    public StoredProcedureService(DatabaseConfig dbConfig)
    {
        _connectionString = dbConfig.GetConnectionString(); 
    }

    public void EjecutarSp(string spName)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand(spName, connection);
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.ExecuteNonQuery();
    }
}