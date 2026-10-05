using Support.Domain.Enums;
using Support.Domain.SharedKernel;

namespace Support.Domain.Errors;

/// <summary>
/// Errores de negocio de las incidencias: uno por cada regla. Cada error lleva un código estable
/// (por ejemplo <c>incident.not_found</c>) y un tipo que la API traduce a un código HTTP.
/// </summary>
public static class IncidentErrors
{
    /// <summary>Se devuelve si el título está vacío o en blanco. Tipo <c>Validation</c> (HTTP 400).</summary>
    public static readonly Error TitleRequired =
        Error.Validation("incident.title_required", "El título es obligatorio.");

    /// <summary>Se devuelve si no se indica quién reporta la incidencia. Tipo <c>Validation</c> (HTTP 400).</summary>
    public static readonly Error ReporterRequired =
        Error.Validation("incident.reporter_required", "Hay que indicar quién reporta la incidencia.");

    /// <summary>Se devuelve al escalar sin referencia del ticket del proveedor. Tipo <c>Validation</c> (HTTP 400).</summary>
    public static readonly Error VendorRefRequired =
        Error.Validation("incident.vendor_ref_required", "Falta la referencia del ticket del proveedor.");

    /// <summary>Se devuelve cuando no existe la incidencia pedida. Tipo <c>NotFound</c> (HTTP 404).</summary>
    public static readonly Error NotFound =
        Error.NotFound("incident.not_found", "La incidencia no existe.");

    /// <summary>
    /// Se devuelve cuando el estado actual no permite la acción pedida (por ejemplo, cerrar una incidencia no resuelta).
    /// Tipo <c>Conflict</c> (HTTP 409).
    /// </summary>
    /// <param name="from">Estado actual de la incidencia.</param>
    /// <param name="to">Estado al que se intentaba pasar.</param>
    /// <returns>Un error que incluye ambos estados en el mensaje.</returns>
    public static Error InvalidTransition(IncidentStatus from, IncidentStatus to) =>
        Error.Conflict("incident.invalid_transition", $"No se puede pasar de {from} a {to}.");
}
