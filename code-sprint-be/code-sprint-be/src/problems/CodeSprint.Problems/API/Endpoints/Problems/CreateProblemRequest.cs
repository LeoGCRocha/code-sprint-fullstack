namespace CodeSprint.Problems.API.Endpoints.Problems;

public record CreateExampleRequest(string Input, string Output, string? Explanation);

public record CreateProblemRequest(
    string? Slug,
    string Title,
    string Difficulty,
    int Points,
    int EstimatedMinutes,
    List<string> Tags,
    string Description,
    List<string> Notes,
    List<string> InputFormat,
    List<string> Constraints,
    List<CreateExampleRequest> Examples);
