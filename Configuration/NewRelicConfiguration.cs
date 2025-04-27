using System.Data;
using Microsoft.Data.SqlClient;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Extensión para leer la clave de New Relic desde la tabla Parameter
    /// y volcarla en IConfiguration en memoria.
    /// </summary>
    public static class NewRelicConfiguration
    {
        private const string ParameterName = "NewRelicLicenseKey";

        /// <summary>
        /// Consulta la tabla Parameter usando ADO.NET y, si encuentra valor,
        /// lo añade a <see cref="IConfiguration"/> como "NewRelic:LicenseKey".
        /// </summary>
        public static void AddNewRelicLicenseFromDatabase(this WebApplicationBuilder builder)
        {
            var connString = builder.Configuration.GetConnectionString("DefaultConnection");
            string? licenseKey = null;

            try
            {
                using var conn = new SqlConnection(connString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"
                    SELECT TOP 1 ParameterValue
                      FROM Parameter
                     WHERE ParameterName = @name";
                cmd.Parameters.AddWithValue("@name", ParameterName);

                licenseKey = cmd.ExecuteScalar() as string;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: no se pudo leer NewRelicLicenseKey: {ex.Message}");
            }

            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                builder.Configuration
                       .AddInMemoryCollection(new Dictionary<string, string?>
                       {
                            ["NewRelic:LicenseKey"] = licenseKey
                       });
            }
        }
    }
}
