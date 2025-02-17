using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Atributo de validación para asegurar que el nombre cumpla con el formato requerido.
    /// 
    /// Para configuraciones de tipo StoredProcedure o VistaSql se permite:
    /// - Letras, dígitos, guiones (-) y guiones bajos (_)
    /// - No se permiten espacios ni las combinaciones "-_" o "_-"
    /// 
    /// Para configuraciones de tipo EndPoint se permite:
    /// - Letras, dígitos, dos puntos (:), barras (/), guiones (-), guiones bajos (_), puntos (.) y comas (,)
    /// - No se permiten espacios.
    /// </summary>
    public class NombreFormatAttribute : ValidationAttribute
    {
        private static readonly Regex RegexPattern = new Regex(@"^(?!.*(?:-_|_-))[A-Za-z0-9](?:[A-Za-z0-9]|[-_](?=[A-Za-z0-9]))*$");

        private static readonly Regex EndpointPattern = new Regex(@"^(?!.*\s)[A-Za-z0-9:/\-_.,]+$");

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var nombre = value as string;
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return ValidationResult.Success;
            }

            var tipoProp = validationContext.ObjectInstance.GetType().GetProperty("Tipo");
            if (tipoProp != null)
            {
                var tipoValue = tipoProp.GetValue(validationContext.ObjectInstance);
                if (tipoValue is TipoConfiguracion tipo && tipo == TipoConfiguracion.EndPoint)
                {
                    if (!EndpointPattern.IsMatch(nombre))
                    {
                        return new ValidationResult("El formato del nombre para un endpoint no es válido. Se permiten letras, dígitos, ':', '/', '-', '_', '.', ',' y no se permiten espacios.");
                    }
                    return ValidationResult.Success;
                }
            }
            
            if (!RegexPattern.IsMatch(nombre))
            {
                return new ValidationResult("El formato del nombre no es válido. Se permiten letras, dígitos, guiones (-) y guiones bajos (_), sin espacios ni las combinaciones '-_' o '_-'.");
            }

            return ValidationResult.Success;
        }
    }
}