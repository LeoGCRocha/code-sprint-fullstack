using System.Text.RegularExpressions;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Users.Domain;

public sealed partial class Handle : ValueObject
{
    public string Value { get; }

    private Handle(string value)
    {
        Value = value;
    }

    [GeneratedRegex("^[a-z0-9_]{3,20}$")]
    private static partial Regex HandleRegex();

    public static Result<Handle> Create(string? baseHandle)
    {
        if (string.IsNullOrWhiteSpace(baseHandle))
            return Error.Validation("handle.empty", "Handle is required");

        var value = baseHandle.Trim().TrimStart('@').ToLowerInvariant();

        if (!HandleRegex().IsMatch(value))
            return Error.Validation("handle.invalid", "Handle must be 3-20 chars: a-z, 0-9, underscore");

        return new Handle(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
