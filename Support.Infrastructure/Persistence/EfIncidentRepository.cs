using Microsoft.EntityFrameworkCore;
using Support.Domain.Entities;
using Support.Domain.Enums;
using Support.Domain.Ports.Driven;
using Support.Domain.ValueObjects;

namespace Support.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio de incidencias con EF Core. No llama a <c>SaveChanges</c>:
/// de confirmar se encarga la unidad de trabajo desde el caso de uso.
/// </summary>
public sealed class EfIncidentRepository(AppDbContext db) : IIncidentRepository
{
    /// <summary>Busca una incidencia por id CON seguimiento de cambios, para poder modificarla.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La incidencia o <c>null</c> si no existe.</returns>
    public Task<Incident?> GetByIdAsync(IncidentId id, CancellationToken ct) =>
        db.Incidents.FirstOrDefaultAsync(i => i.Id == id, ct);

    /// <summary>Lista incidencias SIN seguimiento de cambios (más rápido), de la más reciente a la más antigua.</summary>
    /// <param name="status">Filtro opcional por estado.</param>
    /// <param name="priority">Filtro opcional por prioridad.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// La lista filtrada. Se espera que los filtros se apliquen en la consulta SQL y no en memoria.
    /// </returns>
    public async Task<IReadOnlyList<Incident>> ListAsync(
        IncidentStatus? status, IncidentPriority? priority, CancellationToken ct)
    {
        var query = db.Incidents.AsNoTracking().AsQueryable();
        if (status is not null) query = query.Where(i => i.Status == status);
        if (priority is not null) query = query.Where(i => i.Priority == priority);

        return await query.OrderByDescending(i => i.CreatedAtUtc).ToListAsync(ct);
    }

    /// <summary>Marca la incidencia como nueva en el contexto. Se guardará al confirmar la unidad de trabajo.</summary>
    /// <param name="incident">Incidencia a añadir.</param>
    /// <param name="ct">Token de cancelación.</param>
    public async Task AddAsync(Incident incident, CancellationToken ct) =>
        await db.Incidents.AddAsync(incident, ct);
}
