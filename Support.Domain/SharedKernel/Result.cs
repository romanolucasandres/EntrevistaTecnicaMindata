namespace Support.Domain.SharedKernel
{
    /// <summary>
    /// Resultado de una operación que puede fallar: éxito o fallo con su <see cref="Error"/>. Los fallos ESPERADOS del negocio
    /// viajan como valor (no como excepción); quien llama debe mirar <see cref="IsSuccess"/> antes de seguir.
    /// </summary>
    public class Result
    {
        /// <summary>Constructor protegido: los resultados solo se crean con <c>Success</c> y <c>Failure</c>.</summary>
        /// <param name="isSuccess">Indica si la operación fue correcta.</param>
        /// <param name="error">El error si falló; <see cref="Error.None"/> si fue correcta.</param>
        protected Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public Error Error { get; }
        public bool IsFailure => !IsSuccess;

        /// <summary>Crea un resultado correcto, sin valor.</summary>
        /// <returns>Un resultado con <c>IsSuccess = true</c> y <see cref="Error.None"/>.</returns>
        public static Result Success() => new(true, Error.None);
        /// <summary>Crea un resultado fallido, sin valor.</summary>
        /// <param name="error">Qué falló.</param>
        /// <returns>Un resultado con <c>IsSuccess = false</c> y el error indicado.</returns>
        public static Result Failure(Error error) => new(false, error);
        /// <summary>Crea un resultado correcto que lleva un valor.</summary>
        /// <typeparam name="T">Tipo del valor.</typeparam>
        /// <param name="value">El valor producido por la operación.</param>
        /// <returns>Un resultado correcto; su <c>Value</c> es el indicado.</returns>
        public static Result<T> Success<T>(T value) => new(value, true, Error.None);
        /// <summary>Crea un resultado fallido para una operación que, de ir bien, habría devuelto un valor.</summary>
        /// <typeparam name="T">Tipo del valor que se esperaba.</typeparam>
        /// <param name="error">Qué falló.</param>
        /// <returns>Un resultado fallido. Leer su <c>Value</c> lanza <see cref="InvalidOperationException"/>.</returns>
        public static Result<T> Failure<T>(Error error) => new(default!, false, error);
    }

    /// <summary>Resultado que, si fue correcto, lleva un valor de tipo <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">Tipo del valor.</typeparam>
    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        /// <summary>Constructor interno: se crea con <c>Result.Success&lt;T&gt;</c> o <c>Result.Failure&lt;T&gt;</c>.</summary>
        /// <param name="value">Valor (ignorado si falló).</param>
        /// <param name="isSuccess">Si la operación fue correcta.</param>
        /// <param name="error">El error si falló.</param>
        internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;
        /// <summary>
        /// Valor del resultado. Solo se puede leer si la operación fue correcta; si falló, lanza <see cref="InvalidOperationException"/>.
        /// </summary>
        public T Value => IsSuccess ? _value! : throw new InvalidOperationException("No se puede leer el valor de un resultado fallido.");
    }
}
