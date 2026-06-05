using System.Text.RegularExpressions;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain;

public sealed partial class Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value) => Value = value;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    public static Result<Slug> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Error.Validation("slug.empty", "Slug is required");

        if (raw.Length > 100)
            return Error.Validation("slug.tooLong", "Slug must be 100 characters or fewer");

        if (!SlugRegex().IsMatch(raw))
            return Error.Validation("slug.invalid",
                "Slug must be lowercase alphanumeric with single hyphens between words");

        return new Slug(raw);
    }

    public static Result<Slug> Generate(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("slug.emptyTitle", "Title is required to generate a slug");

        var normalized = title.Trim().ToLowerInvariant();
        var slugged = NonAlphanumericRegex().Replace(normalized, "-").Trim('-');

        while (slugged.Contains("--"))
            slugged = slugged.Replace("--", "-");

        return Create(slugged);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
