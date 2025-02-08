using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Atributo de validación para asegurar que el nombre cumpla con el formato requerido:
    /// - No se permiten espacios.
    /// - Se permiten letras, dígitos, guiones (-) y guiones bajos (_).
    /// - Los guiones y guiones bajos deben estar intercalados entre caracteres alfanuméricos.
    /// - No se permiten las combinaciones "-_" o "_-".
    /// </summary>
    public class NombreFormatAttribute : ValidationAttribute
    {
        private static readonly Regex RegexPattern = new Regex("^(?!.*(?:-_|_-))[A-Za-z0-9](?:[A-Za-z0-9]|[-_](?=[A-Za-z0-9]))*$");

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var nombre = value as string;
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return ValidationResult.Success;
            }

            if (!RegexPattern.IsMatch(nombre))
            {
                return new ValidationResult("El formato del nombre no es válido. Se permiten letras, dígitos, guiones (-) y guiones bajos (_), sin espacios ni las combinaciones '-_' o '_-'.");
            }

            return ValidationResult.Success;
        }
    }
}