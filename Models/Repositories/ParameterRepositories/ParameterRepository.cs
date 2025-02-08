using Microsoft.EntityFrameworkCore;
using SPOrchestratorAPI.Data;
using SPOrchestratorAPI.Models.Entities;

namespace SPOrchestratorAPI.Models.Repositories.ParameterRepositories;

/// <summary>
/// Implementa el repositorio para la entidad <see cref="Parameter"/> utilizando Entity Framework Core.
/// </summary>
public class ParameterRepository : IParameterRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Crea una nueva instancia de <see cref="ParameterRepository"/>.
    /// </summary>
    /// <param name="context">El contexto de datos de la aplicación.</param>
    public ParameterRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Parameter> CreateAsync(Parameter parameter)
    {
        if (parameter == null)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        _context.Parameters.Add(parameter);
        await _context.SaveChangesAsync();
        return parameter;
    }

    /// <inheritdoc />
    public async Task<Parameter?> GetByIdAsync(int parameterId)
    {
        return await _context.Parameters.FirstOrDefaultAsync(p => p.ParameterId == parameterId);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public async Task<Parameter?> GetByNameAsync(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("El nombre del parámetro es requerido.", nameof(parameterName));
        }

        return await _context.Parameters.FirstOrDefaultAsync(
            p => p.ParameterName.ToLower() == parameterName.ToLower());
    }
}
