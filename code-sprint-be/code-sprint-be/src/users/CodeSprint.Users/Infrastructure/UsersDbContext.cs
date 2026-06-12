using CodeSprint.Shared.Messaging;
using CodeSprint.Users.Domain;
using CodeSprint.Users.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Users.Infrastructure;

/// <summary>
/// Persistence boundary for the Users bounded context. Mappings live in
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes in this assembly
/// (e.g. <see cref="UserConfiguration"/>) and are applied automatically.
/// </summary>
public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    /// <summary>Consumer-side deduplication for the inbox pattern (shared mapping).</summary>
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<HeatmapDay> HeatmapDays => Set<HeatmapDay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
        modelBuilder.AddInbox(); // shared inbox_messages table, into the "users" schema

        base.OnModelCreating(modelBuilder);
    }
}
