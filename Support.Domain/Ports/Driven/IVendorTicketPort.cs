using Support.Domain.Entities;
using Support.Domain.SharedKernel;

namespace Support.Domain.Ports.Driven;

/// <summary>
/// Puerto de salida hacia el proveedor tecnológico. El dominio define qué necesita; la infraestructura provee un
/// adaptador falso hoy y uno HTTP real el día que exista la API del proveedor.
/// </summary>
public interface IVendorTicketPort
{
    /// <summary>Abre un ticket en el sistema del proveedor para la incidencia dada.</summary>
    /// <param name="incident">Incidencia que se escala.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>
    /// Se espera un resultado correcto con la referencia del ticket (por ejemplo <c>VND-1A2B3C4D</c>).
    /// Si el proveedor no responde o rechaza la petición, un fallo con su error.
    /// </returns>
    Task<Result<string>> OpenTicketAsync(Incident incident, CancellationToken ct);
}
