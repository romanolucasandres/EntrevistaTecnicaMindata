namespace Support.Application.Abstractions
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Confirma en una sola transacción todos los cambios pendientes. Los repositorios solo añaden y consultan;
        /// los cambios no se guardan hasta que se llama a este método.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación (opcional).</param>
        /// <returns>El número de registros afectados. Si algo falla, no se guarda ninguno (todo o nada).</returns>
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
