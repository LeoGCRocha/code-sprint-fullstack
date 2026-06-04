namespace CodeSprint.Shared.Ids;

/// <summary>Identity of a User aggregate (Users bounded context).</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.CreateVersion7());

    public static UserId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
