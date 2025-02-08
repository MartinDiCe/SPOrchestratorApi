using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Clase que provee una fábrica para generar respuestas de error personalizadas cuando falla el model binding.
    /// </summary>
    public static class ModelValidationResponseFactory
    {
        /// <summary>
        /// Método que se utiliza como InvalidModelStateResponseFactory.
        /// Personaliza la respuesta cuando falla el model binding, en especial para el campo "parameters".
        /// </summary>
        public static IActionResult CustomResponse(ModelStateDictionary modelState)
        {
            var parameterErrors = modelState
                .Where(e => e.Key.Contains("parameters", System.StringComparison.OrdinalIgnoreCase))
                .SelectMany(e => e.Value.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            if (parameterErrors.Any())
            {
                var customResponse = new
                {
                    message = "El campo 'parameters' no tiene el formato correcto. Se espera que sea un objeto JSON, por ejemplo: { 'FECHADESDE': 'yyyy-MM-dd', 'FECHAHASTA': 'yyyy-MM-dd', 'IDOPERACION': valor }.",
                    errors = parameterErrors
                };

                return new BadRequestObjectResult(customResponse);
            }
            
            return new BadRequestObjectResult(modelState);
        }
    }
}