using Microsoft.EntityFrameworkCore;
using Support.Application.Abstractions;
using Support.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Support.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de base de datos de la aplicación (EF Core). Además actúa como unidad de trabajo: implementa <see cref="IUnitOfWork"/>.
    /// </summary>
    /// <param name="options">Opciones del contexto (proveedor de base de datos y cadena de conexión).</param>
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
    {
        /// <summary>Tabla de incidencias.</summary>
        public DbSet<Incident> Incidents => Set<Incident>();

        /// <summary>Implementa la unidad de trabajo: guarda todos los cambios pendientes del contexto en una sola transacción.</summary>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>El número de filas afectadas.</returns>
        public Task<int> CommitAsync(CancellationToken ct) => SaveChangesAsync(ct);

        /// <summary>
        /// Configura el modelo aplicando todas las configuraciones de entidad (<c>IEntityTypeConfiguration</c>) del ensamblado que contiene este contexto.
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo de EF Core.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    }
}
