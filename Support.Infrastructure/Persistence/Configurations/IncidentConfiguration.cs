using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Support.Domain.Entities;
using Support.Domain.ValueObjects;

namespace Support.Infrastructure.Persistence.Configurations;

/// <summary>Mapeo de la entidad <see cref="Incident"/> a la tabla <c>Incidents</c> de SQL Server.</summary>
public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    /// <summary>Define tabla, clave, conversiones, longitudes e índices.</summary>
    /// <param name="b">Constructor de la configuración de la entidad.</param>
    /// <remarks>
    /// Se espera: la clave es un <see cref="IncidentId"/> convertido a <c>Guid</c> y generado fuera de la base de datos;
    /// los enums se guardan como texto (legibles con SQL: <c>Open</c>, no <c>1</c>); y hay un índice por
    /// <c>(Status, Priority)</c> que acelera el filtro más habitual.
    /// </remarks>
    public void Configure(EntityTypeBuilder<Incident> b)
    {
        b.ToTable("Incidents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new IncidentId(value))
            .ValueGeneratedNever();

        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        b.Property(x => x.ReportedBy).HasMaxLength(200).IsRequired();
        b.Property(x => x.VendorTicketRef).HasMaxLength(100);
        b.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        b.HasIndex(x => new { x.Status, x.Priority });
    }
}
