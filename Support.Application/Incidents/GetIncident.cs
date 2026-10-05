using Support.Application.Dtos;
using Support.Domain.Errors;
using Support.Domain.Ports.Driven;
using Support.Domain.SharedKernel;
using Support.Domain.ValueObjects;

namespace Support.Application.Incidents;

/// <summary>Caso de uso: consultar una incidencia por su id. Es de solo lectura: no confirma nada.</summary>
public sealed class GetIncidentHandler(IIncidentRepository incidents)
{
    /// <summary>Busca la incidencia y la devuelve como DTO.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Se espera un resultado correcto con el <see cref="IncidentDto"/>. Si no existe, falla con
    /// <see cref="IncidentErrors.NotFound"/> (404).
    /// </returns>
    public async Task<Result<IncidentDto>> HandleAsync(IncidentId id, CancellationToken ct)
    {
        var incident = await incidents.GetByIdAsync(id, ct);
        return incident is null
            ? Result.Failure<IncidentDto>(IncidentErrors.NotFound)
            : Result.Success(IncidentDto.From(incident));
    }
}
