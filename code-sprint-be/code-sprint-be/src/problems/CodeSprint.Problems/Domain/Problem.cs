using CodeSprint.Problems.Domain.Events;
using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain;

public sealed class Problem : AggregateRoot<ProblemId>
{
    private readonly List<Example> _examples = [];
    private readonly List<Tag> _tags = [];
    private readonly List<TestCase> _testCases = [];

    private List<string> _notes = [];
    private List<string> _inputFormat = [];
    private List<string> _constraints = [];

    private Problem(
        ProblemId id,
        Slug slug,
        string title,
        Difficulty difficulty,
        int points,
        int estimatedMinutes,
        string description) : base(id)
    {
        Slug = slug;
        Title = title;
        Difficulty = difficulty;
        Points = points;
        EstimatedMinutes = estimatedMinutes;
        Description = description;
        IsPublished = false;
    }

    public Slug Slug { get; private set; }
    public string Title { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public int Points { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public string Description { get; private set; }
    public bool IsPublished { get; private set; }

    public IReadOnlyList<string> Notes => _notes.AsReadOnly();
    public IReadOnlyList<string> InputFormat => _inputFormat.AsReadOnly();
    public IReadOnlyList<string> Constraints => _constraints.AsReadOnly();
    public IReadOnlyList<Example> Examples => _examples.AsReadOnly();
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyList<TestCase> TestCases => _testCases.AsReadOnly();

    public static Result<Problem> Create(
        Slug slug,
        string title,
        Difficulty difficulty,
        int points,
        int estimatedMinutes,
        IReadOnlyList<Tag> tags,
        string description,
        IReadOnlyList<string> notes,
        IReadOnlyList<string> inputFormat,
        IReadOnlyList<string> constraints,
        IReadOnlyList<Example> examples)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("problem.title.empty", "Title is required");
        if (points <= 0)
            return Error.Validation("problem.points.invalid", "Points must be greater than 0");
        if (estimatedMinutes <= 0)
            return Error.Validation("problem.estimatedMinutes.invalid", "Estimated minutes must be greater than 0");
        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("problem.description.empty", "Description is required");
        if (examples.Count == 0)
            return Error.Validation("problem.examples.empty", "At least one example is required");

        var problem = new Problem(ProblemId.New(), slug, title, difficulty, points, estimatedMinutes, description);

        problem._notes = [..notes];
        problem._inputFormat = [..inputFormat];
        problem._constraints = [..constraints];
        problem._examples.AddRange(examples.Select((e, i) => new Example(i + 1, e.Input, e.Output, e.Explanation)));
        problem._tags.AddRange(tags);
        problem.Raise(new ProblemCreated(problem.Id));

        return problem;
    }

    public Result SetExamples(IReadOnlyList<Example> examples)
    {
        if (examples.Count == 0)
            return Result.Failure(Error.Validation("problem.examples.empty", "At least one example is required"));

        _examples.Clear();
        _examples.AddRange(examples.Select((e, i) => new Example(i + 1, e.Input, e.Output, e.Explanation)));
        return Result.Success();
    }

    public void SetTestCases(IReadOnlyList<TestCase> testCases)
    {
        _testCases.Clear();
        _testCases.AddRange(testCases.Select((tc, i) => new TestCase(i + 1, tc.Input, tc.ExpectedOutput, tc.IsHidden)));
    }

    public Result Publish()
    {
        if (_testCases.Count == 0)
            return Result.Failure(Error.Validation("problem.publish.noTestCases", "Cannot publish a problem with no test cases"));

        IsPublished = true;
        Raise(new ProblemPublished(Id));
        return Result.Success();
    }

    public void Edit(
        string title,
        Difficulty difficulty,
        int points,
        int estimatedMinutes,
        IReadOnlyList<Tag> tags,
        string description,
        IReadOnlyList<string> notes,
        IReadOnlyList<string> inputFormat,
        IReadOnlyList<string> constraints)
    {
        Title = title;
        Difficulty = difficulty;
        Points = points;
        EstimatedMinutes = estimatedMinutes;
        Description = description;
        _notes = [..notes];
        _inputFormat = [..inputFormat];
        _constraints = [..constraints];
        _tags.Clear();
        _tags.AddRange(tags);
    }
}
