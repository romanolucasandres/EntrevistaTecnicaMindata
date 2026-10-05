using Microsoft.AspNetCore.Mvc;
using Support.Domain.SharedKernel;

namespace Support.Api.Extensions
{
    /// <summary>Extensiones para traducir los errores del dominio a respuestas HTTP.</summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Convierte un <see cref="Error"/> en una respuesta HTTP con el código que corresponde a su tipo y un <c>ProblemDetails</c> en el cuerpo.
        /// </summary>
        /// <param name="error">Error de negocio que se quiere traducir.</param>
        /// <returns>Una respuesta con el código HTTP del tipo de error (400, 401, 403, 404, 409 o 500) y un <c>ProblemDetails</c> con su código y mensaje.</returns>
        public static IActionResult ToProblem(this Error error)
        {
            var status = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,

                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            return new ObjectResult(new ProblemDetails
            {
                Status = status,
                Title = error.Code,
                Detail = error.Message
            })
            {
                StatusCode = status
            };
        }
    }
}
