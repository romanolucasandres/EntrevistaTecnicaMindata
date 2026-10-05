using Support.Application.Abstractions;
using Support.Application.Dtos;
using Support.Domain.Entities;
using Support.Domain.Errors;
using Support.Domain.Ports.Driven;
using Support.Domain.SharedKernel;
using Support.Domain.ValueObjects;

namespace Support.Application.Incidents;

/// <summary>
/// Base común de las acciones que cambian el estado de una incidencia. Todas siguen el mismo patrón:
/// cargar, aplicar la acción del dominio y confirmar.
/// </summary>
public abstract class IncidentTransitionHandler(IIncidentRepository incidents, IUnitOfWork uow)
{
    /// <summary>Carga la incidencia, ejecuta la acción y, solo si tuvo éxito, confirma y devuelve el DTO.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="action">Acción del dominio a aplicar sobre la incidencia cargada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Se espera un resultado correcto con el DTO actualizado. Si no existe, <see cref="IncidentErrors.NotFound"/> (404);
    /// si la acción falla, su error (normalmente 409). Si algo falla, NO se confirma ningún cambio.
    /// </returns>
    protected async Task<Result<IncidentDto>> ApplyAsync(
        IncidentId id, Func<Incident, Task<Result>> action, CancellationToken ct)
    {
        var incident = await incidents.GetByIdAsync(id, ct);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        var result = await action(incident);
        if (result.IsFailure) return Result.Failure<IncidentDto>(result.Error);

        await uow.CommitAsync(ct);
        return Result.Success(IncidentDto.From(incident));
    }
}

/// <summary>Caso de uso: empezar a trabajar una incidencia (<c>Open → InProgress</c>).</summary>
public sealed class StartIncidentHandler(IIncidentRepository incidents, IUnitOfWork uow)
    : IncidentTransitionHandler(incidents, uow)
{
    /// <summary>Pone la incidencia en curso.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El DTO en estado <c>InProgress</c>; 404 si no existe; 409 si no estaba <c>Open</c>.</returns>
    public Task<Result<IncidentDto>> HandleAsync(IncidentId id, CancellationToken ct) =>
        ApplyAsync(id, i => Task.FromResult(i.StartWork()), ct);
}

/// <summary>Caso de uso: escalar una incidencia al proveedor (<c>InProgress → EscalatedToVendor</c>).</summary>
public sealed class EscalateIncidentHandler(IIncidentRepository incidents, IUnitOfWork uow, IVendorTicketPort vendor)
    : IncidentTransitionHandler(incidents, uow)
{
    /// <summary>
    /// Escala la incidencia. El orden es importante: primero se comprueba la regla, después se llama al proveedor
    /// y por último se cambia el estado. Así nunca se abre un ticket externo para algo que no se puede escalar.
    /// </summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Se espera el DTO en estado <c>EscalatedToVendor</c> con la referencia del ticket. 404 si no existe;
    /// 409 si no estaba <c>InProgress</c> (y el proveedor NO se llama); el error del proveedor si este falla.
    /// </returns>
    public Task<Result<IncidentDto>> HandleAsync(IncidentId id, CancellationToken ct) =>
        ApplyAsync(id, async incident =>
        {
            // 1) La regla primero: si no se puede escalar, no abrimos nada en el proveedor.
            var can = incident.CanEscalate();
            if (can.IsFailure) return can;

            // 2) Llamada externa a través del puerto.
            var ticket = await vendor.OpenTicketAsync(incident, ct);
            if (ticket.IsFailure) return Result.Failure(ticket.Error);

            // 3) Cambio de estado en el dominio.
            return incident.EscalateToVendor(ticket.Value);
        }, ct);
}

/// <summary>Caso de uso: dar una incidencia por resuelta (<c>InProgress</c> o <c>EscalatedToVendor</c> → <c>Resolved</c>).</summary>
public sealed class ResolveIncidentHandler(IIncidentRepository incidents, IUnitOfWork uow)
    : IncidentTransitionHandler(incidents, uow)
{
    /// <summary>Marca la incidencia como resuelta.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El DTO en estado <c>Resolved</c>; 404 si no existe; 409 si el estado no lo permite.</returns>
    public Task<Result<IncidentDto>> HandleAsync(IncidentId id, CancellationToken ct) =>
        ApplyAsync(id, i => Task.FromResult(i.Resolve()), ct);
}

/// <summary>Caso de uso: cerrar una incidencia (<c>Resolved → Closed</c>).</summary>
public sealed class CloseIncidentHandler(IIncidentRepository incidents, IUnitOfWork uow, TimeProvider clock)
    : IncidentTransitionHandler(incidents, uow)
{
    /// <summary>Cierra la incidencia y registra la fecha de cierre con la hora del reloj inyectado.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// El DTO en estado <c>Closed</c> con <c>ClosedAtUtc</c> informada; 404 si no existe;
    /// 409 si no estaba <c>Resolved</c> (por ejemplo, al cerrarla dos veces).
    /// </returns>
    public Task<Result<IncidentDto>> HandleAsync(IncidentId id, CancellationToken ct) =>
        ApplyAsync(id, i => Task.FromResult(i.Close(clock.GetUtcNow().UtcDateTime)), ct);
}
