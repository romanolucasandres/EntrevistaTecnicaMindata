namespace Support.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de una incidencia. Flujo permitido:
/// <c>Open → InProgress → (EscalatedToVendor) → Resolved → Closed</c>. Cualquier otra transición se rechaza.
/// </summary>
public enum IncidentStatus
{
    /// <summary>Recién registrada; nadie ha empezado a trabajarla.</summary>
    Open = 1,

    /// <summary>Un agente la está trabajando.</summary>
    InProgress = 2,

    /// <summary>Trasladada al proveedor tecnológico, con la referencia de su ticket.</summary>
    EscalatedToVendor = 3,

    /// <summary>Solucionada, a la espera de confirmación del usuario.</summary>
    Resolved = 4,

    /// <summary>Cerrada de forma definitiva, con fecha de cierre.</summary>
    Closed = 5
}
