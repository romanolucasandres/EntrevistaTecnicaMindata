using Support.Domain.Entities;
using Support.Domain.Ports.Driven;
using Support.Domain.SharedKernel;

namespace Support.Infrastructure.ExternalServices;

/// <summary>
/// Adaptador FALSO del sistema de tickets del proveedor. Sirve para desarrollar y probar sin proveedor real.
/// En producción se sustituye por un adaptador HTTP cambiando una sola línea de inyección de dependencias.
/// </summary>
public sealed class FakeVendorTicketAdapter : IVendorTicketPort
{
    /// <summary>Simula la apertura de un ticket.</summary>
    /// <param name="incident">Incidencia que se escala (no se usa en el fake).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Siempre un resultado correcto con una referencia de la forma <c>VND-XXXXXXXX</c> (8 hexadecimales en mayúscula).</returns>
    public Task<Result<string>> OpenTicketAsync(Incident incident, CancellationToken ct)
    {
        var reference = $"VND-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        return Task.FromResult(Result.Success(reference));
    }
}
