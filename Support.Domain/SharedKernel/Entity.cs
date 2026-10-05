namespace Support.Domain.SharedKernel
{
    /// <summary>
    /// Clase base de las entidades del dominio: aporta un identificador único de tipo <typeparamref name="TId"/> y la igualdad por id.
    /// </summary>
    /// <typeparam name="TId">Tipo del identificador (por ejemplo, <c>IncidentId</c>).</typeparam>
    public abstract class Entity<TId> where TId : notnull
    {
        public TId Id { get; protected set; } = default!;
        /// <summary>Crea la entidad con su identidad.</summary>
        /// <param name="id">Identificador de la entidad.</param>
        protected Entity(TId id) => Id = id;
        // Requerido por EF Core para materializar la entidad.
        protected Entity() { }
        /// <summary>Dos entidades son la misma si son del mismo tipo y tienen el mismo id, aunque sean instancias distintas.</summary>
        /// <param name="obj">Objeto con el que comparar.</param>
        /// <returns><c>true</c> si es una entidad del mismo tipo con el mismo id.</returns>
        public override bool Equals(object? obj) =>
        obj is Entity<TId> other && GetType() == other.GetType() && Id.Equals(other.Id);
        /// <summary>Código hash basado en el id, coherente con <see cref="Equals(object?)"/>.</summary>
        /// <returns>El hash del identificador.</returns>
        public override int GetHashCode() => Id.GetHashCode();

    }
}
