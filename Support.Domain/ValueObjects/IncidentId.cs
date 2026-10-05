namespace Support.Domain.ValueObjects;

/// <summary>
/// Identificador tipado de una <see cref="Support.Domain.Entities.Incident"/>. Envuelve un <see cref="Guid"/> para que
/// el compilador impida mezclarlo con el id de otra entidad.
/// </summary>
/// <remarks>
/// El <c>Guid</c> solo debe aparecer en los bordes (URL, JSON y base de datos). El dominio no inventa ids:
/// se los entrega <c>IIdGenerator</c>.
/// </remarks>
/// <param name="Value">El <see cref="Guid"/> subyacente.</param>
public readonly record struct IncidentId(Guid Value);
