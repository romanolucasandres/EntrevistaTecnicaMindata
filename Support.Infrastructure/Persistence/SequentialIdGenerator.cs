using Microsoft.EntityFrameworkCore.ValueGeneration;
using Support.Application.Abstractions;
using Support.Domain.ValueObjects;

namespace Support.Infrastructure.Persistence;

/// <summary>
/// Generador de ids con Guids secuenciales. Un <c>Guid.NewGuid()</c> aleatorio fragmenta el índice de la clave
/// primaria en SQL Server; los secuenciales no. La estrategia depende de la base de datos, por eso vive aquí.
/// </summary>
public sealed class SequentialIdGenerator : IIdGenerator
{
    private static readonly SequentialGuidValueGenerator Generator = new();

    /// <summary>Genera el id de una incidencia nueva.</summary>
    /// <returns>Un <see cref="IncidentId"/> único cuyo valor crece con el tiempo (apto para índices clustered).</returns>
    public IncidentId NewIncidentId() => new(Generator.Next(null!));
}
