using Support.Domain.Enums;
using Support.Domain.Errors;
using Support.Domain.SharedKernel;
using Support.Domain.ValueObjects;

namespace Support.Domain.Entities;

/// <summary>
/// Incidencia de soporte. Es la entidad raíz del caso: concentra las reglas de negocio y su ciclo de vida
/// (<c>Open → InProgress → [EscalatedToVendor] → Resolved → Closed</c>).
/// </summary>
/// <remarks>
/// Todos los <c>set</c> son privados: el estado solo cambia por los métodos de esta clase, que aplican las reglas.
/// Los fallos de negocio se devuelven como <see cref="Result"/>, no como excepciones.
/// </remarks>
public sealed class Incident : Entity<IncidentId>
{
    /// <summary>Título corto de la incidencia. Obligatorio; se guarda sin espacios sobrantes.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Descripción del problema, tal y como la explica quien reporta.</summary>
    public string Description { get; private set; } = default!;

    /// <summary>Prioridad asignada.</summary>
    public IncidentPriority Priority { get; private set; }

    /// <summary>Estado actual dentro del ciclo de vida.</summary>
    public IncidentStatus Status { get; private set; }

    /// <summary>Persona que reportó la incidencia. Obligatorio.</summary>
    public string ReportedBy { get; private set; } = default!;

    /// <summary>Referencia del ticket abierto en el proveedor. Solo tiene valor si la incidencia se escaló.</summary>
    public string? VendorTicketRef { get; private set; }

    /// <summary>Instante de creación, en UTC.</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Instante de cierre, en UTC. Solo tiene valor si la incidencia está cerrada.</summary>
    public DateTime? ClosedAtUtc { get; private set; }

    /// <summary>Constructor sin parámetros que necesita EF Core para reconstruir la entidad. No usar desde el negocio.</summary>
    private Incident() { }

    /// <summary>Constructor privado: la única vía de creación es <see cref="Create"/>, que valida los datos.</summary>
    /// <param name="id">Identificador de la incidencia.</param>
    private Incident(IncidentId id) : base(id) { }

    /// <summary>
    /// Crea una incidencia nueva en estado <see cref="IncidentStatus.Open"/>. Nunca existe una incidencia inválida:
    /// si los datos no cumplen las reglas, se devuelve un fallo y no se crea nada.
    /// </summary>
    /// <param name="id">Id ya generado por quien llama (el dominio no genera ids).</param>
    /// <param name="title">Título. No puede estar vacío ni en blanco.</param>
    /// <param name="description">Descripción. Si llega nula se guarda vacía.</param>
    /// <param name="priority">Prioridad inicial.</param>
    /// <param name="reportedBy">Quién reporta. No puede estar vacío ni en blanco.</param>
    /// <param name="nowUtc">Instante de creación en UTC (el dominio no consulta el reloj).</param>
    /// <returns>
    /// Se espera un resultado correcto con la incidencia <c>Open</c>. Falla con
    /// <see cref="IncidentErrors.TitleRequired"/> o <see cref="IncidentErrors.ReporterRequired"/>.
    /// </returns>
    public static Result<Incident> Create(
        IncidentId id, string title, string description, IncidentPriority priority, string reportedBy, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title)) return Result.Failure<Incident>(IncidentErrors.TitleRequired);
        if (string.IsNullOrWhiteSpace(reportedBy)) return Result.Failure<Incident>(IncidentErrors.ReporterRequired);

        return Result.Success(new Incident(id)
        {
            Title = title.Trim(),
            Description = (description ?? string.Empty).Trim(),
            Priority = priority,
            Status = IncidentStatus.Open,
            ReportedBy = reportedBy.Trim(),
            CreatedAtUtc = nowUtc
        });
    }

    /// <summary>Un agente empieza a trabajar la incidencia: <c>Open → InProgress</c>.</summary>
    /// <returns>
    /// Se espera éxito si está <c>Open</c>. En cualquier otro estado falla con
    /// <see cref="IncidentErrors.InvalidTransition"/> y no cambia nada.
    /// </returns>
    public Result StartWork() => Transition(IncidentStatus.InProgress, IncidentStatus.Open);

    /// <summary>
    /// Pregunta si la incidencia se puede escalar, SIN modificarla. Permite que el caso de uso compruebe la regla
    /// antes de llamar al proveedor externo, para no abrir un ticket que luego no se pueda usar.
    /// </summary>
    /// <returns>Éxito si está <c>InProgress</c>; si no, <see cref="IncidentErrors.InvalidTransition"/>.</returns>
    public Result CanEscalate() => Status == IncidentStatus.InProgress
        ? Result.Success()
        : Result.Failure(IncidentErrors.InvalidTransition(Status, IncidentStatus.EscalatedToVendor));

    /// <summary>Marca la incidencia como trasladada al proveedor: <c>InProgress → EscalatedToVendor</c>.</summary>
    /// <param name="vendorTicketRef">Referencia del ticket abierto en el proveedor. No puede estar vacía; se guarda recortada.</param>
    /// <returns>
    /// Se espera éxito si está <c>InProgress</c> y hay referencia. Falla con
    /// <see cref="IncidentErrors.InvalidTransition"/> o <see cref="IncidentErrors.VendorRefRequired"/>, sin cambiar nada.
    /// </returns>
    public Result EscalateToVendor(string vendorTicketRef)
    {
        var can = CanEscalate();
        if (can.IsFailure) return can;
        if (string.IsNullOrWhiteSpace(vendorTicketRef)) return Result.Failure(IncidentErrors.VendorRefRequired);

        Status = IncidentStatus.EscalatedToVendor;
        VendorTicketRef = vendorTicketRef.Trim();
        return Result.Success();
    }

    /// <summary>Da la incidencia por solucionada: <c>InProgress</c> o <c>EscalatedToVendor</c> → <c>Resolved</c>.</summary>
    /// <returns>Se espera éxito desde esos dos estados; en cualquier otro falla con <see cref="IncidentErrors.InvalidTransition"/>.</returns>
    public Result Resolve() =>
        Transition(IncidentStatus.Resolved, IncidentStatus.InProgress, IncidentStatus.EscalatedToVendor);

    /// <summary>Cierra la incidencia de forma definitiva: <c>Resolved → Closed</c>, y registra la fecha de cierre.</summary>
    /// <param name="nowUtc">Instante de cierre en UTC.</param>
    /// <returns>
    /// Se espera éxito solo si está <c>Resolved</c> (y entonces <see cref="ClosedAtUtc"/> queda informada).
    /// Si no, falla con <see cref="IncidentErrors.InvalidTransition"/> y la fecha no se toca.
    /// </returns>
    public Result Close(DateTime nowUtc)
    {
        var result = Transition(IncidentStatus.Closed, IncidentStatus.Resolved);
        if (result.IsSuccess) ClosedAtUtc = nowUtc;
        return result;
    }

    /// <summary>Aplica una transición de estado comprobando que el estado actual sea uno de los permitidos.</summary>
    /// <param name="to">Estado de destino.</param>
    /// <param name="allowedFrom">Estados desde los que se permite la transición.</param>
    /// <returns>Éxito (y el estado cambia) si el actual está permitido; si no, <see cref="IncidentErrors.InvalidTransition"/>.</returns>
    private Result Transition(IncidentStatus to, params IncidentStatus[] allowedFrom)
    {
        if (!allowedFrom.Contains(Status))
            return Result.Failure(IncidentErrors.InvalidTransition(Status, to));

        Status = to;
        return Result.Success();
    }
}
