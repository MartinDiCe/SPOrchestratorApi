using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

namespace SPOrchestratorAPI.Configuration
{
    public static class NewRelicInstaller
    {
        private const string ToggleName = "newRelicEnabled";

        public static WebApplicationBuilder AddNewRelicIfEnabled(this WebApplicationBuilder builder)
        {
            using var tmp = builder.Services.BuildServiceProvider();
            var repo   = tmp.GetRequiredService<IParameterRepository>();
            var toggle = repo.GetByNameAsync(ToggleName)
                .GetAwaiter().GetResult()
                ?.ParameterValue;

            if (toggle?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
            {
                builder.AddNewRelicLicenseFromDatabase();

                LoggingConfigurator.Configure(builder);
            }
            else
            {
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
            }

            return builder;
        }
    }
}