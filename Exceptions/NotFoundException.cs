using System;

namespace SPOrchestratorAPI.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}