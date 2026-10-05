using Support.Domain.Entities;

namespace Support.Application.Dtos;

/// <summary>
/// Contrato público de una incidencia hacia el exterior. La API nunca expone la entidad: el DTO puede cambiar
/// sin tocar el dominio.
/// </summary>
/// <param name="Id">Identificador (aquí sí aparece el <see cref="Guid"/>: es el borde).</param>
/// <param name="Title">Título.</param>
/// <param name="Description">Descripción.</param>
/// <param name="Priority">Prioridad como texto (<c>High</c>, no un número).</param>
/// <param name="Status">Estado como texto.</param>
/// <param name="ReportedBy">Quién la reportó.</param>
/// <param name="VendorTicketRef">Referencia del ticket del proveedor, si se escaló.</param>
/// <param name="CreatedAtUtc">Creación en UTC.</param>
/// <param name="ClosedAtUtc">Cierre en UTC, si está cerrada.</param>
public sealed record IncidentDto(
    Guid Id, string Title, string Description, string Priority, string Status,
    string ReportedBy, string? VendorTicketRef, DateTime CreatedAtUtc, DateTime? ClosedAtUtc)
{
    /// <summary>Convierte una entidad en su DTO.</summary>
    /// <param name="i">Incidencia de origen. No puede ser nula.</param>
    /// <returns>El DTO con los mismos datos; los enums salen como texto.</returns>
    public static IncidentDto From(Incident i) => new(
        i.Id.Value, i.Title, i.Description, i.Priority.ToString(), i.Status.ToString(),
        i.ReportedBy, i.VendorTicketRef, i.CreatedAtUtc, i.ClosedAtUtc);
}
