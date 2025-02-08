using System.ComponentModel.DataAnnotations;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Validations;

public class ConnectionStringValidationAttribute : ValidationAttribute
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer; 

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var connectionString = value as string;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new ValidationResult("La cadena de conexión es obligatoria.");
        }

        try
        {
            switch (Provider)
            {
                case DatabaseProvider.SqlServer:
                    var sqlBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                    break;
                case DatabaseProvider.MySql:
                    var mySqlBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString);
                    break;
                case DatabaseProvider.PostgreSql:
                    var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                    break;
                case DatabaseProvider.Oracle:
                    var oracleBuilder = new Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder(connectionString);
                    break;
                default:
                    throw new NotSupportedException("Proveedor de base de datos no soportado.");
            }
        }
        catch (Exception ex)
        {
            return new ValidationResult(
                $"La cadena de conexión no es válida. El formato esperado es: " + 
                "\"Server=xxx.xxx.x.x;Database=DataBaseName;User Id=userdb;Password=passwordDb;TrustServerCertificate=True;");
        }

        return ValidationResult.Success;
    }
}