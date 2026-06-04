using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Users.Domain
{
    public class User : AggregateRoot<UserId>
    {
        // Owned navigations (Identity) cannot be bound through the constructor, so
        // EF materializes the scalars here and sets Identity via its property.
        public User(UserId id, Email email, Handle handle, string displayName, string? avatar, DateTime memberSince) : base(id)
        {
            Email = email;
            Handle = handle;
            DisplayName = displayName;
            Avatar = avatar;
            MemberSince = memberSince;
        }

        public Email Email { get; private set; }
        public Handle Handle { get; private set; }
        public string DisplayName { get; private set; }
        public string? Avatar { get; private set; }
        public AuthIdentity Identity { get; private set; } = null!;
        public DateTime MemberSince { get; private set; }

        /// <summary>
        /// Creates a new user from a first successful authentication. The provider
        /// identity is fixed for the lifetime of the account (one identity per user).
        /// </summary>
        public static User Register(
            Email email,
            Handle handle,
            string displayName,
            string? avatar,
            string provider,
            string providerSubject)
            => new(UserId.New(), email, handle, displayName, avatar, DateTime.UtcNow)
            {
                Identity = new AuthIdentity(provider, providerSubject),
            };
    }
}