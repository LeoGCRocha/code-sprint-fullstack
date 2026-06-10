using CodeSprint.Shared.Messaging;
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

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("submissions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubmissionsDbContext).Assembly);

        // The outbox mapping lives in the Shared assembly, so the assembly scan
        // above does not pick it up — add it explicitly. It inherits the default
        // "submissions" schema set above.
        modelBuilder.AddOutbox();

        base.OnModelCreating(modelBuilder);
    }
}
