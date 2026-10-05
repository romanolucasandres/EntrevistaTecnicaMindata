namespace Support.Domain.SharedKernel
{
    /// <summary>Tipo de error. La API lo traduce a un código HTTP (400, 401, 403, 404, 409, 500) sin mirar textos.</summary>
    public enum ErrorType
    {
        None,
        NotFound,
        Validation,
        Unauthorized,
        Forbidden,
        Conflict,
        InternalServerError
    }
    /// <summary>
    /// Error esperado del negocio o de la aplicación: lleva un código estable, un mensaje para personas y un tipo que la API traduce a HTTP.
    /// </summary>
    /// <param name="Code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
    /// <param name="Message">Mensaje para personas.</param>
    /// <param name="Type">Tipo de error: determina el código HTTP de la respuesta.</param>
    public sealed record Error(string Code, string Message, ErrorType Type)
    {
        /// <summary>Ausencia de error. Es el error de todo resultado correcto.</summary>
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
        /// <summary>Crea un error de tipo <c>NotFound</c> (HTTP 404): no existe el recurso pedido.</summary>
        /// <param name="code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
        /// <param name="message">Mensaje para personas.</param>
        /// <returns>El error, listo para devolverse en un resultado fallido.</returns>
        public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
        /// <summary>Crea un error de tipo <c>Validation</c> (HTTP 400): los datos de entrada no son válidos.</summary>
        /// <param name="code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
        /// <param name="message">Mensaje para personas.</param>
        /// <returns>El error, listo para devolverse en un resultado fallido.</returns>
        public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
        /// <summary>Crea un error de tipo <c>Unauthorized</c> (HTTP 401): no hay una identidad válida (no autenticado).</summary>
        /// <param name="code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
        /// <param name="message">Mensaje para personas.</param>
        /// <returns>El error, listo para devolverse en un resultado fallido.</returns>
        public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
        /// <summary>Crea un error de tipo <c>Forbidden</c> (HTTP 403): hay identidad pero no permiso para la acción.</summary>
        /// <param name="code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
        /// <param name="message">Mensaje para personas.</param>
        /// <returns>El error, listo para devolverse en un resultado fallido.</returns>
        public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
        /// <summary>Crea un error de tipo <c>Conflict</c> (HTTP 409): el estado actual no permite la acción.</summary>
        /// <param name="code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
        /// <param name="message">Mensaje para personas.</param>
        /// <returns>El error, listo para devolverse en un resultado fallido.</returns>
        public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
        /// <summary>Crea un error de tipo <c>InternalServerError</c> (HTTP 500): fallo inesperado del servidor.</summary>
        /// <param name="code">Código estable y legible por máquina (por ejemplo <c>incident.not_found</c>).</param>
        /// <param name="message">Mensaje para personas.</param>
        /// <returns>El error, listo para devolverse en un resultado fallido.</returns>
        public static Error InternalServerError(string code, string message) => new(code, message, ErrorType.InternalServerError);
    }
}
