namespace CloneUZ.Api.Common;

public enum ErrorKind { Validation, NotFound, Conflict }

public sealed record Error(ErrorKind Kind, string Code, string Message, IReadOnlyDictionary<string, object?>? Details = null)
{
    public static Error Validation(string code, string message) => new(ErrorKind.Validation, code, message);
    public static Error NotFound(string code, string message) => new(ErrorKind.NotFound, code, message);
    public static Error Conflict(string code, string message, IReadOnlyDictionary<string, object?>? details = null) =>
        new(ErrorKind.Conflict, code, message, details);
}

/// <summary>
/// Ожидаемые бизнес-отказы («место занято») — результат, а не исключение.
/// На пике продаж «место занято» — самый частый ответ, а исключения дорогие.
/// </summary>
public readonly struct Result<T>
{
    private readonly T? _value;

    private Result(T value) { _value = value; Error = null; }
    private Result(Error error) { _value = default; Error = error; }

    public Error? Error { get; }
    public bool IsSuccess => Error is null;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException($"Result is failed: {Error!.Code}");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}
