namespace CodeSprint.Shared.Ids;

/// <summary>Identity of a TestSuite aggregate (Problems bounded context).</summary>
public readonly record struct TestSuiteId(Guid Value)
{
    public static TestSuiteId New() => new(Guid.CreateVersion7());

    public static TestSuiteId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
