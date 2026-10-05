using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Support.Application
{
    /// <summary>
    /// Métodos de extensión para registrar en la inyección de dependencias los servicios de la capa de aplicación.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra los casos de uso (todas las clases concretas cuyo nombre acaba en <c>Handler</c>) y los validadores de FluentValidation.
        /// Añadir un caso de uso nuevo no obliga a tocar este método.
        /// </summary>
        /// <param name="services">Colección de servicios.</param>
        /// <returns>La misma colección, para encadenar llamadas.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            foreach (var type in assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
           t.Name.EndsWith("Handler")))
                services.AddScoped(type);

            services.AddValidatorsFromAssembly(assembly);
            return services;
        }
    }
}
