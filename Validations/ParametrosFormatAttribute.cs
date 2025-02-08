using System.ComponentModel.DataAnnotations;

namespace SPOrchestratorAPI.Validations
{
    /// <summary>
    /// Atributo de validación para el formato de parámetros. 
    /// Verifica que el string no contenga espacios y se espere que los parámetros estén separados por el carácter ';'.
    /// </summary>
    public class ParametrosFormatAttribute : ValidationAttribute
    {
        /// <summary>
        /// Valida el valor especificado, asegurando que no contenga espacios.
        /// </summary>
        /// <param name="value">El valor a validar.</param>
        /// <param name="validationContext">El contexto de validación.</param>
        /// <returns>
        /// Un objeto <see cref="ValidationResult"/> que indica si la validación fue exitosa o no.
        /// Si el valor es nulo o vacío, se considera válido.
        /// Si el valor contiene espacios, se devuelve un <see cref="ValidationResult"/> con el mensaje de error.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var parametros = value as string;
            if (string.IsNullOrWhiteSpace(parametros))
            {
                return ValidationResult.Success;
            }

            if (parametros.Contains(" "))
            {
                return new ValidationResult("No se permiten espacios en los parámetros. Separe los nombres de parámetros con ';' sin espacios.");
            }

            return ValidationResult.Success;
        }
    }
}