using Support.Domain.ValueObjects;

namespace Support.Application.Abstractions;

/// <summary>
/// Puerto de plataforma: genera los ids. El dominio no inventa ids; quien crea una entidad se los pide a este puerto,
/// y Infrastructure decide la estrategia (por ejemplo, Guids secuenciales para SQL Server).
/// </summary>
public interface IIdGenerator
{
    /// <summary>Genera el id para una incidencia nueva.</summary>
    /// <returns>Un <see cref="IncidentId"/> único.</returns>
    IncidentId NewIncidentId();
}
