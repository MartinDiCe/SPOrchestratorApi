using Hangfire.Dashboard;

namespace SPOrchestratorAPI.Admin;

/// <summary>
/// Temporal hasta conectar con una gateway y microservicios de auth
/// </summary>
class BasicAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext ctx)
    {
        var http = ctx.GetHttpContext();
        
        return http.User.Identity?.IsAuthenticated ?? false
            || CheckBasicAuth(http);
    }

    static bool CheckBasicAuth(HttpContext ctx)
    {
        if (!ctx.Request.Headers.TryGetValue("Authorization", out var hdr)) return false;
        var cred = hdr.ToString().Split(' ').Last();                   
        var bytes = Convert.FromBase64String(cred);
        var text  = System.Text.Encoding.UTF8.GetString(bytes);         
        var parts = text.Split(':');
        return parts[0] == "admin" && parts[1] == "4dm1n";
    }
}
