namespace SPOrchestratorAPI.Exceptions;

/// <summary>
/// Excepción personalizada para indicar que un recurso no fue encontrado en la base de datos.
/// </summary>
public class ResourceNotFoundException(string message) : Exception(message);