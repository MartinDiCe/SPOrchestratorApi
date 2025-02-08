namespace SPOrchestratorAPI.Exceptions;

/// <summary>
/// Excepción personalizada para errores específicos de servicio.
/// </summary>
[Serializable]
public class ServiceException : Exception
{
    /// <summary>
    /// Código de error asociado a la excepción.
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// Constructor por defecto para la excepción.
    /// </summary>
    public ServiceException() { }

    /// <summary>
    /// Constructor con un mensaje personalizado.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    public ServiceException(string message) : base(message) { }

    /// <summary>
    /// Constructor con un mensaje personalizado y un código de error.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    /// <param name="errorCode">Código de error.</param>
    public ServiceException(string message, int errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Constructor para excepciones con detalles adicionales.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    /// <param name="innerException">Excepción interna.</param>
    public ServiceException(string message, Exception innerException) : base(message, innerException) { }
}