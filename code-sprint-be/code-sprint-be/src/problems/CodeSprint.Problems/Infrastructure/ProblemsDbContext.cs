using CodeSprint.Problems.Domain;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Problems.Infrastructure;

/// <summary>
/// Persistence boundary for the Problems bounded context. Mappings live in
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes in this assembly
/// (e.g. <see cref="ProblemConfiguration"/>) and are applied automatically.
/// </summary>
public sealed class ProblemsDbContext(DbContextOptions<ProblemsDbContext> options) : DbContext(options)
{
    public DbSet<Problem> Problems => Set<Problem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("problems");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProblemsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
