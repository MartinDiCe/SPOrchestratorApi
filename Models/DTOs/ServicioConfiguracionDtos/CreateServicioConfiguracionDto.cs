﻿using System.ComponentModel.DataAnnotations;
using SPOrchestratorAPI.Helpers;
using SPOrchestratorAPI.Models.Enums;
using SPOrchestratorAPI.Validations;

namespace SPOrchestratorAPI.Models.DTOs.ServicioConfiguracionDtos;

/// <summary>
/// DTO para la creación de una nueva configuración de <see cref="ServicioConfiguracion"/>,
/// asociada a un <see cref="Servicio"/> existente.
/// </summary>
public class CreateServicioConfiguracionDto
{
    /// <summary>
    /// Identificador del <see cref="Servicio"/> al que pertenece esta configuración.
    /// </summary>
    public int ServicioId { get; set; }

    /// <summary>
    /// Nombre del procedimiento almacenado (Stored Procedure) que se ejecutará.
    /// </summary>
    [Required]
    [NombreFormat]
    public string NombreProcedimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cadena de conexión a la base de datos donde se ubica el procedimiento.
    /// </summary>
    [ConnectionStringValidation]
    public string ConexionBaseDatos { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros opcionales (en formato string) requeridos para el Stored Procedure.
    /// Debe contener los nombres separados por ';' sin espacios.
    /// </summary>
    [ParametrosFormat]
    public string? Parametros { get; set; }

    /// <summary>
    /// Máximo número de reintentos permitidos antes de declarar un fallo en la ejecución.
    /// </summary>
    public int MaxReintentos { get; set; }

    /// <summary>
    /// Tiempo máximo de espera (en segundos) antes de cancelar la ejecución del SP.
    /// </summary>
    public int TimeoutSegundos { get; set; }
    
    /// <summary>
    /// Base de datos de la conexion.
    /// </summary>
    [Required]
    public DatabaseProvider Provider { get; set; }
}