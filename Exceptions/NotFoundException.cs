using System;

namespace SPOrchestratorAPI.Exceptions;

public abstract class NotFoundException(string message) : Exception(message);