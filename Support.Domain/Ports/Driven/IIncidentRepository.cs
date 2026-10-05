using Support.Domain.Entities;
using Support.Domain.Enums;
using Support.Domain.ValueObjects;

namespace Support.Domain.Ports.Driven;

/// <summary>
/// Puerto de salida: lo que el negocio necesita para guardar y consultar incidencias. Lo implementa Infrastructure.
/// El repositorio NO confirma los cambios; de eso se encarga <c>IUnitOfWork</c>.
/// </summary>
public interface IIncidentRepository
{
    /// <summary>Busca una incidencia por su id, con seguimiento de cambios (para poder modificarla y confirmar después).</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La incidencia, o <c>null</c> si no existe (no lanza excepción).</returns>
    Task<Incident?> GetByIdAsync(IncidentId id, CancellationToken ct);

    /// <summary>Lista incidencias para consulta (sin seguimiento de cambios), de la más reciente a la más antigua.</summary>
    /// <param name="status">Si tiene valor, solo devuelve incidencias en ese estado.</param>
    /// <param name="priority">Si tiene valor, solo devuelve incidencias con esa prioridad.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La lista filtrada; vacía si no hay coincidencias.</returns>
    Task<IReadOnlyList<Incident>> ListAsync(IncidentStatus? status, IncidentPriority? priority, CancellationToken ct);

    /// <summary>Marca una incidencia nueva para guardarla. No persiste hasta que se confirme la unidad de trabajo.</summary>
    /// <param name="incident">Incidencia a añadir.</param>
    /// <param name="ct">Token de cancelación.</param>
    Task AddAsync(Incident incident, CancellationToken ct);
}
