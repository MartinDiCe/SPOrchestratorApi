using SPOrchestratorAPI.Data;

namespace SPOrchestratorAPI.Configuration;

public static class DatabaseInitializer
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.EnsureCreated();
        
    }
}