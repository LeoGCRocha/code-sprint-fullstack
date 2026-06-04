namespace CodeSprint.Shared.Ids;

/// <summary>Identity of a Problem aggregate (Problems bounded context).</summary>
public readonly record struct ProblemId(Guid Value)
{
    public static ProblemId New() => new(Guid.CreateVersion7());

    public static ProblemId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
