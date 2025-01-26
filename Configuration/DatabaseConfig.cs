namespace SPOrchestratorAPI.Configuration;

using Microsoft.Extensions.Configuration;

public class DatabaseConfig(IConfiguration configuration)
{
    public string GetConnectionString() => 
        configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");

}