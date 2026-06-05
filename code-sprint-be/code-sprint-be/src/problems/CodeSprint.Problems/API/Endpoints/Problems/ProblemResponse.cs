namespace CodeSprint.Problems.API.Endpoints.Problems;

public record ExampleResponse(string Input, string Output, string? Explanation);

public record ProblemResponse(
    string Id,
    string Slug,
    string Title,
    string Difficulty,
    int Points,
    string EstimatedTime,
    List<string> Tags,
    string Description,
    List<string> Notes,
    List<string> InputFormat,
    List<string> Constraints,
    List<ExampleResponse> Examples,
    int SolvedCount);
