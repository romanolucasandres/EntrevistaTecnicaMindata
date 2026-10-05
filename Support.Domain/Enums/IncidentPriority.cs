namespace Support.Domain.Enums;

/// <summary>
/// Prioridad de una incidencia, resultado de combinar impacto y urgencia (de menor a mayor).
/// </summary>
public enum IncidentPriority
{
    /// <summary>Poco impacto y sin urgencia.</summary>
    Low = 1,

    /// <summary>Impacto o urgencia moderados.</summary>
    Medium = 2,

    /// <summary>Alto impacto o urgencia; debe atenderse pronto.</summary>
    High = 3,

    /// <summary>Servicio caído o bloqueo grave; máxima prioridad.</summary>
    Critical = 4
}
