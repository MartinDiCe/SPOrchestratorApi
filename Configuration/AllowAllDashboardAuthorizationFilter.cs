using Hangfire.Dashboard;

namespace SPOrchestratorAPI.Configuration;

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}