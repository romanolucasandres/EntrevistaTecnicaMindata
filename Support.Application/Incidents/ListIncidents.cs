using Support.Application.Dtos;
using Support.Domain.Enums;
using Support.Domain.Ports.Driven;

namespace Support.Application.Incidents;

/// <summary>Caso de uso: listar incidencias, con filtros opcionales. Es de solo lectura.</summary>
public sealed class ListIncidentsHandler(IIncidentRepository incidents)
{
    /// <summary>Consulta las incidencias y las devuelve como DTOs, de la más reciente a la más antigua.</summary>
    /// <param name="status">Filtro opcional por estado.</param>
    /// <param name="priority">Filtro opcional por prioridad.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La lista filtrada; vacía si no hay coincidencias (nunca es un error).</returns>
    public async Task<IReadOnlyList<IncidentDto>> HandleAsync(
        IncidentStatus? status, IncidentPriority? priority, CancellationToken ct)
    {
        var list = await incidents.ListAsync(status, priority, ct);
        return list.Select(IncidentDto.From).ToList();
    }
}
