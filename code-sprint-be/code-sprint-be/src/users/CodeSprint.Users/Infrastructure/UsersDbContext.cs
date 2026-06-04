using CodeSprint.Users.Domain;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
