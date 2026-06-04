namespace CodeSprint.Shared.Primitives;

/// <summary>Classifies an <see cref="Error"/> so the API edge can map it to a transport status.</summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
}

/// <summary>
/// A domain failure carried by <see cref="Result"/>. Prefer returning these
/// over throwing for expected, recoverable outcomes (validation, not-found,
/// uniqueness conflicts).
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}
