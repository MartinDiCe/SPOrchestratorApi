using Hangfire.Dashboard;

namespace SPOrchestratorAPI.Configuration
{
    /// <summary>
    /// Authorization filter for Hangfire Dashboard that allows all requests.
    /// Implement this when you want to expose the dashboard without authentication.
    /// </summary>
    public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Determines whether the current HTTP context is authorized to access the Hangfire Dashboard.
        /// </summary>
        /// <param name="context">Contextual information about the dashboard request.</param>
        /// <returns>
        /// <c>true</c> to allow any user to access the dashboard; otherwise, <c>false</c>.
        /// </returns>
        public bool Authorize(DashboardContext context) => true;
    }
}