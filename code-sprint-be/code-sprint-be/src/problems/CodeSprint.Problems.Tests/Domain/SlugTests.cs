using CodeSprint.Problems.Domain;

namespace CodeSprint.Problems.Tests.Domain;

public class SlugTests
{
    [Theory]
    [InlineData("two-sum")]
    [InlineData("valid-parentheses")]
    [InlineData("a")]
    [InlineData("abc-123")]
    public void Create_ValidSlug_Succeeds(string raw)
    {
        var result = Slug.Create(raw);
        Assert.True(result.IsSuccess);
        Assert.Equal(raw, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Has Spaces")]
    [InlineData("UPPERCASE")]
    [InlineData("-leading-hyphen")]
    [InlineData("trailing-hyphen-")]
    [InlineData("double--hyphen")]
    public void Create_InvalidSlug_ReturnsFailure(string? raw)
    {
        var result = Slug.Create(raw);
        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("Two Sum", "two-sum")]
    [InlineData("Valid Parentheses!", "valid-parentheses")]
    [InlineData("3Sum", "3sum")]
    [InlineData("Longest Substring Without Repeating Characters", "longest-substring-without-repeating-characters")]
    [InlineData("  extra   spaces  ", "extra-spaces")]
    public void Generate_FromTitle_ProducesExpectedSlug(string title, string expected)
    {
        var result = Slug.Generate(title);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Generate_EmptyTitle_ReturnsFailure()
    {
        var result = Slug.Generate("");
        Assert.True(result.IsFailure);
    }
}
