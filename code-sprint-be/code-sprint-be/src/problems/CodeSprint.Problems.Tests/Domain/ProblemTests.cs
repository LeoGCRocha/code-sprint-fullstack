using CodeSprint.Problems.Domain;
using CodeSprint.Problems.Domain.Events;

namespace CodeSprint.Problems.Tests.Domain;

public class ProblemTests
{
    private static Slug ValidSlug() => Slug.Create("two-sum").Value;
    private static List<Example> OneExample() =>
        [new Example(1, "input", "output", null)];

    [Fact]
    public void Create_ValidArgs_Succeeds()
    {
        var result = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy,
            points: 50, estimatedMinutes: 10,
            tags: [Tag.Create("Math").Value],
            description: "A problem",
            notes: ["Note 1"],
            inputFormat: ["Line 1: N"],
            constraints: ["1 <= N <= 100"],
            examples: OneExample());

        Assert.True(result.IsSuccess);
        Assert.Equal("Two Sum", result.Value.Title);
        Assert.Equal(50, result.Value.Points);
        Assert.False(result.Value.IsPublished);
        Assert.Single(result.Value.DomainEvents.OfType<ProblemCreated>());
    }

    [Fact]
    public void Create_ZeroPoints_ReturnsFailure()
    {
        var result = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy,
            points: 0, estimatedMinutes: 10,
            tags: [],
            description: "desc",
            notes: [],
            inputFormat: [],
            constraints: [],
            examples: OneExample());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_NoExamples_ReturnsFailure()
    {
        var result = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy,
            points: 50, estimatedMinutes: 10,
            tags: [],
            description: "desc",
            notes: [],
            inputFormat: [],
            constraints: [],
            examples: []);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void SetExamples_EmptyList_ReturnsFailure()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        var result = problem.SetExamples([]);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void SetExamples_AssignsOrdinalsOneIndexed()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        problem.SetExamples([
            new Example(0, "a", "b", null),
            new Example(0, "c", "d", null),
        ]);

        Assert.Equal(1, problem.Examples[0].Ordinal);
        Assert.Equal(2, problem.Examples[1].Ordinal);
    }

    [Fact]
    public void Publish_WithTestCases_RaisesProblemPublished()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        problem.SetTestCases([new TestCase(1, "in", "out", true)]);
        problem.ClearDomainEvents();

        var result = problem.Publish();

        Assert.True(result.IsSuccess);
        Assert.True(problem.IsPublished);
        Assert.Single(problem.DomainEvents.OfType<ProblemPublished>());
    }

    [Fact]
    public void Publish_WithNoTestCases_ReturnsFailure()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        var result = problem.Publish();

        Assert.True(result.IsFailure);
    }
}
