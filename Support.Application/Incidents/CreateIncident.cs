using FluentValidation;
using Support.Application.Abstractions;
using Support.Application.Dtos;
using Support.Domain.Entities;
using Support.Domain.Enums;
using Support.Domain.Ports.Driven;
using Support.Domain.SharedKernel;

namespace Support.Application.Incidents;

/// <summary>Datos necesarios para crear una incidencia (cuerpo de la petición).</summary>
/// <param name="Title">Título. Obligatorio, máximo 200 caracteres.</param>
/// <param name="Description">Descripción. Obligatoria, máximo 4000 caracteres.</param>
/// <param name="Priority">Prioridad. Debe ser un valor válido del enum.</param>
/// <param name="ReportedBy">Quién reporta. Obligatorio, máximo 200 caracteres.</param>
public sealed record CreateIncidentCommand(string Title, string Description, IncidentPriority Priority, string ReportedBy);

/// <summary>Valida la FORMA de lo que llega (vacíos y longitudes). Las reglas de negocio las valida el dominio.</summary>
public sealed class CreateIncidentValidator : AbstractValidator<CreateIncidentCommand>
{
    /// <summary>Define las reglas: título, descripción y reportante obligatorios y con longitud máxima; prioridad válida.</summary>
    public CreateIncidentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ReportedBy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

/// <summary>Caso de uso: crear una incidencia.</summary>
public sealed class CreateIncidentHandler(
    IIncidentRepository incidents,
    IUnitOfWork uow,
    TimeProvider clock,
    IIdGenerator ids,
    IValidator<CreateIncidentCommand> validator)
{
    /// <summary>
    /// Valida la entrada, crea la incidencia con un id y una hora generados fuera del dominio, la añade al repositorio
    /// y confirma con una sola operación.
    /// </summary>
    /// <param name="command">Datos de la incidencia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Se espera un resultado correcto con el <see cref="IncidentDto"/> en estado <c>Open</c>. Falla con
    /// <c>incident.invalid</c> (400) si el validador rechaza los datos, o con el error del dominio
    /// (por ejemplo, título en blanco). Si falla, no se guarda nada.
    /// </returns>
    public async Task<Result<IncidentDto>> HandleAsync(CreateIncidentCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<IncidentDto>(Error.Validation("incident.invalid", message));
        }

        var created = Incident.Create(
            ids.NewIncidentId(), command.Title, command.Description, command.Priority, command.ReportedBy,
            clock.GetUtcNow().UtcDateTime);
        if (created.IsFailure) return Result.Failure<IncidentDto>(created.Error);

        await incidents.AddAsync(created.Value, ct);
        await uow.CommitAsync(ct);

        return Result.Success(IncidentDto.From(created.Value));
    }
}
