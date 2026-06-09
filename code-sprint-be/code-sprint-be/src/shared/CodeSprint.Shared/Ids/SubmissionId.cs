namespace CodeSprint.Shared.Ids;

/// <summary>Identity of a Submission aggregate (Submissions bounded context).</summary>
public readonly record struct SubmissionId(Guid Value)
{
    public static SubmissionId New() => new(Guid.CreateVersion7());

    public static SubmissionId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
