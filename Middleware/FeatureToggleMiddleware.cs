using SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

namespace SPOrchestratorAPI.Middleware
{
    /// <summary>
    /// Bloquea o deja pasar la request según el valor de un parámetro en BD.
    /// </summary>
    public class FeatureToggleMiddleware(RequestDelegate next, string parameterName)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var repo = context.RequestServices.GetRequiredService<IParameterRepository>();
            var param = await repo.GetByNameAsync(parameterName);

            if (param == null ||
                !param.ParameterValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next(context);
        }
    }
}