using System.Text.RegularExpressions;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Users.Domain;

public sealed partial class Email : ValueObject
{
    public string Address { get; }
    public string LocalPart { get; }
    public string Domain { get; }

    private Email(string address, string localPart, string domain)
    {
        Address = address;
        LocalPart = localPart;
        Domain = domain;
    }
    
    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    private static partial Regex EmailRegex();
    
    public static Result<Email> Create(string? baseAddress)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
            return Error.Validation("email.empty", "Email is required");

        var address =  baseAddress.ToLowerInvariant().Trim();
        
        if (address.Length > 320 || !EmailRegex().IsMatch(address))
            return Error.Validation("email.invalid", "Invalid email address");
        
        var at = baseAddress.IndexOf('@');
        var localPart = baseAddress[..at].ToLowerInvariant();
        var domain = baseAddress[(at + 1)..].ToLowerInvariant();
        
        return new Email($"{localPart}@{domain}", localPart, domain);
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Address;
    }
}