using Microsoft.AspNetCore.Mvc;
using Support.Api.Extensions;
using Support.Application.Dtos;
using Support.Application.Incidents;
using Support.Domain.Enums;
using Support.Domain.SharedKernel;
using Support.Domain.ValueObjects;

namespace Support.Api.Controllers;

/// <summary>
/// Endpoints REST de incidencias (<c>/api/incidents</c>). El controller es delgado: recibe, delega en el caso de uso
/// y traduce el resultado a HTTP. No contiene reglas de negocio.
/// </summary>
[ApiController]
[Route("api/incidents")]
public class IncidentsController : ControllerBase
{
    /// <summary>GET /api/incidents. Lista incidencias, con filtros opcionales por query string.</summary>
    /// <param name="status">Filtro opcional por estado (por ejemplo <c>?status=Open</c>).</param>
    /// <param name="priority">Filtro opcional por prioridad.</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 con la lista (vacía si no hay coincidencias).</returns>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] IncidentStatus? status, [FromQuery] IncidentPriority? priority,
        [FromServices] ListIncidentsHandler handler, CancellationToken ct) =>
        Ok(await handler.HandleAsync(status, priority, ct));

    /// <summary>GET /api/incidents/{id}. Consulta una incidencia.</summary>
    /// <param name="id">Id de la incidencia (el <see cref="Guid"/> se convierte en <see cref="IncidentId"/> aquí, en el borde).</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 con la incidencia; 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id, [FromServices] GetIncidentHandler handler, CancellationToken ct) =>
        Respond(await handler.HandleAsync(new IncidentId(id), ct));

    /// <summary>POST /api/incidents. Crea una incidencia (queda <c>Open</c>).</summary>
    /// <param name="command">Cuerpo JSON con título, descripción, prioridad y reportante.</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>201 con la incidencia y la ubicación del recurso; 400 si los datos no son válidos.</returns>
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateIncidentCommand command, [FromServices] CreateIncidentHandler handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.Error.ToProblem();
    }

    /// <summary>POST /api/incidents/{id}/start. Empieza a trabajar la incidencia (<c>Open → InProgress</c>).</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 con la incidencia actualizada; 404 si no existe; 409 si no estaba <c>Open</c>.</returns>
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(
        Guid id, [FromServices] StartIncidentHandler handler, CancellationToken ct) =>
        Respond(await handler.HandleAsync(new IncidentId(id), ct));

    /// <summary>POST /api/incidents/{id}/escalate. Abre un ticket en el proveedor y escala la incidencia.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// 200 con la incidencia y la referencia del ticket; 404 si no existe; 409 si no estaba <c>InProgress</c>
    /// (en ese caso no se abre ningún ticket).
    /// </returns>
    [HttpPost("{id:guid}/escalate")]
    public async Task<IActionResult> Escalate(
        Guid id, [FromServices] EscalateIncidentHandler handler, CancellationToken ct) =>
        Respond(await handler.HandleAsync(new IncidentId(id), ct));

    /// <summary>POST /api/incidents/{id}/resolve. Da la incidencia por resuelta.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 con la incidencia <c>Resolved</c>; 404 si no existe; 409 si el estado no lo permite.</returns>
    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(
        Guid id, [FromServices] ResolveIncidentHandler handler, CancellationToken ct) =>
        Respond(await handler.HandleAsync(new IncidentId(id), ct));

    /// <summary>POST /api/incidents/{id}/close. Cierra la incidencia de forma definitiva.</summary>
    /// <param name="id">Id de la incidencia.</param>
    /// <param name="handler">Caso de uso (inyectado).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 con la incidencia <c>Closed</c>; 404 si no existe; 409 si no estaba <c>Resolved</c> (incluido cerrarla dos veces).</returns>
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id, [FromServices] CloseIncidentHandler handler, CancellationToken ct) =>
        Respond(await handler.HandleAsync(new IncidentId(id), ct));

    /// <summary>Traduce el resultado de un caso de uso a HTTP.</summary>
    /// <param name="result">Resultado del caso de uso.</param>
    /// <returns>200 con el DTO si fue correcto; si falló, un <c>ProblemDetails</c> con el código que corresponde al tipo de error.</returns>
    private IActionResult Respond(Result<IncidentDto> result) =>
        result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem();
}
