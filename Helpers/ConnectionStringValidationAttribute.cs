using System;
using System.ComponentModel.DataAnnotations;
using SPOrchestratorAPI.Models.Enums;

namespace SPOrchestratorAPI.Helpers
{
    /// <summary>
    /// Atributo de validación para cadenas de conexión.
    /// Si el objeto validado tiene una propiedad "Tipo" con valor EndPoint o
    /// si la cadena es "No requiere", se omite la validación.
    /// En caso contrario, se verifica que la cadena contenga los elementos mínimos requeridos para SQL Server.
    /// Si falla, se retorna un mensaje unificado indicando el formato esperado.
    /// </summary>
    public class ConnectionStringValidationAttribute : ValidationAttribute
    {
        public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var connectionString = value as string;

            // Si la cadena es "No requiere", se considera válida.
            if (string.Equals(connectionString, "No requiere", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Success;
            }

            // Si el objeto validado tiene una propiedad "Tipo" y su valor es EndPoint, se omite la validación.
            var tipoProp = validationContext.ObjectInstance.GetType().GetProperty("Tipo");
            if (tipoProp != null)
            {
                var tipoValue = tipoProp.GetValue(validationContext.ObjectInstance);
                if (tipoValue is TipoConfiguracion tipo && tipo == TipoConfiguracion.EndPoint)
                {
                    return ValidationResult.Success;
                }
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new ValidationResult("La cadena de conexión es obligatoria.");
            }

            var lowerConnectionString = connectionString.ToLowerInvariant();
            // Lista de claves requeridas para SQL Server
            string[] requiredKeys = new string[]
            {
                "server=",
                "database=",
                "user id=",
                "password=",
                "trustservercertificate="
            };

            foreach (var key in requiredKeys)
            {
                if (!lowerConnectionString.Contains(key))
                {
                    return new ValidationResult(
                        "La cadena de conexión no es válida. El formato esperado es: " +
                        "\"Server=xxx.xxx.x.x;Database=DataBaseName;User Id=userdb;Password=passwordDb;TrustServerCertificate=True;\"");
                }
            }

            return ValidationResult.Success;
        }
    }
}
