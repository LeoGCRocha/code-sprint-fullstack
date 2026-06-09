using CodeSprint.Shared.Primitives;

namespace CodeSprint.Submissions.Domain;

/// <summary>
/// The submitted source code. Validated non-empty and capped at
/// <see cref="MaxLength"/> to blunt large-payload abuse.
/// </summary>
public sealed class SourceCode : ValueObject
{
    /// <summary>Maximum accepted source size, in characters (64 KB).</summary>
    public const int MaxLength = 64 * 1024;

    public string Value { get; }

    private SourceCode(string value) => Value = value;

    public static Result<SourceCode> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Error.Validation("submission.code.empty", "Source code is required");

        if (raw.Length > MaxLength)
            return Error.Validation("submission.code.tooLong",
                $"Source code must be {MaxLength} characters or fewer");

        return new SourceCode(raw);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
