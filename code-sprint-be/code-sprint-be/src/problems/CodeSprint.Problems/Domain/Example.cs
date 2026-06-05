namespace CodeSprint.Problems.Domain;

public sealed class Example
{
    public Example() { }

    public Example(int ordinal, string input, string output, string? explanation)
    {
        Ordinal = ordinal;
        Input = input;
        Output = output;
        Explanation = explanation;
    }

    public int Ordinal { get; init; }
    public string Input { get; init; } = string.Empty;
    public string Output { get; init; } = string.Empty;
    public string? Explanation { get; init; }
}
