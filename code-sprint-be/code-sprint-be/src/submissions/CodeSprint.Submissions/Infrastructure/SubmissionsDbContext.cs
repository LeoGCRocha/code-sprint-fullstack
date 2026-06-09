using CodeSprint.Submissions.Domain;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Submissions.Infrastructure;

/// <summary>
/// Persistence boundary for the Submissions bounded context. Mappings live in
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes in this assembly
/// (e.g. <see cref="SubmissionConfiguration"/>) and are applied automatically.
/// </summary>
public sealed class SubmissionsDbContext(DbContextOptions<SubmissionsDbContext> options) : DbContext(options)
{
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("submissions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubmissionsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
