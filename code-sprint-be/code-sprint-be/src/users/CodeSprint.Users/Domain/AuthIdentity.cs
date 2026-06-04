using CodeSprint.Shared.Primitives;

namespace CodeSprint.Users.Domain;

/// <summary>
/// The external provider identity a user authenticates with. One per user:
/// each provider login (Google, GitHub) is a distinct account.
/// </summary>
public sealed class AuthIdentity : ValueObject
{
    public string Provider { get; private set; }
    public string ProviderSubject { get; private set; }

    public AuthIdentity(string provider, string providerSubject)
    {
        Provider = provider;
        ProviderSubject = providerSubject;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Provider;
        yield return ProviderSubject;
    }
}
