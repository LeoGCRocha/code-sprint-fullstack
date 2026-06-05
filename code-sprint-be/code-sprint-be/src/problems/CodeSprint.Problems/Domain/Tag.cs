using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain;

public sealed class Tag : ValueObject
{
    public string Value { get; }

    private Tag(string value) => Value = value;

    public static Result<Tag> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Error.Validation("tag.empty", "Tag cannot be empty");

        var value = raw.Trim();
        if (value.Length > 50)
            return Error.Validation("tag.tooLong", "Tag must be 50 characters or fewer");

        return new Tag(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
