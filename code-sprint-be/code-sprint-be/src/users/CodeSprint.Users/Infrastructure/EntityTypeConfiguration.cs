using CodeSprint.Shared.Ids;
using CodeSprint.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Users.Infrastructure;

/// <summary>
/// EF Core mapping for the <see cref="User"/> aggregate.
///
/// Strongly-typed id and value objects are mapped with value converters (single
/// columns) rather than owned types because they are constructor parameters —
/// EF cannot bind an owned navigation through the entity constructor, but it can
/// bind a converted scalar. <see cref="AuthIdentity"/> is part of the aggregate
/// and has no repository of its own, so it is mapped as an owned collection.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasConversion(email => email.Address, value => Email.Create(value).Value)
            .HasMaxLength(320)
            .IsRequired();

        // Not unique: one person can hold separate accounts across providers
        // (Google, GitHub) under the same email. Identity uniqueness is enforced
        // on (provider, provider_subject) instead.
        builder.HasIndex(u => u.Email);

        builder.Property(u => u.Handle)
            .HasColumnName("handle")
            .HasConversion(handle => handle.Value, value => Handle.Create(value).Value)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(u => u.Handle).IsUnique();

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Avatar)
            .HasColumnName("avatar")
            .HasMaxLength(2048);

        builder.Property(u => u.MemberSince)
            .HasColumnName("member_since")
            .IsRequired();

        builder.OwnsOne(u => u.Identity, identity =>
        {
            identity.Property(i => i.Provider)
                .HasColumnName("provider")
                .HasMaxLength(100)
                .IsRequired();

            identity.Property(i => i.ProviderSubject)
                .HasColumnName("provider_subject")
                .HasMaxLength(256)
                .IsRequired();

            identity.HasIndex(i => new { i.Provider, i.ProviderSubject }).IsUnique();
        });

        builder.Navigation(u => u.Identity)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}
