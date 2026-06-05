namespace CodeSprint.Problems.Domain;

public sealed class TestCase
{
    public TestCase() { }

    public TestCase(int ordinal, string input, string expectedOutput, bool isHidden)
    {
        Ordinal = ordinal;
        Input = input;
        ExpectedOutput = expectedOutput;
        IsHidden = isHidden;
    }

    public int Ordinal { get; init; }
    public string Input { get; init; } = string.Empty;
    public string ExpectedOutput { get; init; } = string.Empty;
    public bool IsHidden { get; init; }
}
