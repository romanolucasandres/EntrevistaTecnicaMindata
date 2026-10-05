using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Support.Application.Abstractions;
using Support.Domain.Ports.Driven;
using Support.Infrastructure.ExternalServices;
using Support.Infrastructure.Persistence;

namespace Support.Infrastructure
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra lo que implementa los puertos: el contexto de EF Core con SQL Server (cadena <c>ConnectionStrings:Default</c>), la unidad de
        /// trabajo, el repositorio de incidencias, el adaptador del proveedor, el generador de ids y el reloj.
        /// Es el único sitio donde se decide qué implementación va con cada interfaz.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <param name="config">Configuración de la aplicación (de ella se lee la cadena de conexión).</param>
        /// <returns>La misma colección, para encadenar llamadas.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            //IUnitOfWork apunta al mismo AppDbContext de la petición: repositorios y unidad de trabajo comparten una sola instancia.
            //TimeProvider.System es el reloj real. En los tests se cambia por uno falso y la hora queda bajo control.

            services.AddDbContext<AppDbContext>(o =>
           o.UseSqlServer(config.GetConnectionString("Default")));
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IIncidentRepository, EfIncidentRepository>();
            services.AddScoped<IVendorTicketPort, FakeVendorTicketAdapter>();
            services.AddSingleton<IIdGenerator, SequentialIdGenerator>();
            services.AddSingleton(TimeProvider.System);

            return services;
        }

    }
}
