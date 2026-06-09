using CodeSprint.Shared.Ids;
using CodeSprint.Submissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Submissions.Infrastructure;

/// <summary>
/// EF Core mapping for the <see cref="Submission"/> aggregate.
///
/// Strongly-typed ids and the <see cref="SourceCode"/> value object are mapped with
/// value converters (single columns). UserId/ProblemId are stored by value with NO
/// database foreign key — they reference other bounded contexts. The
/// <see cref="Evaluation"/> owned type is table-split into the <c>submissions</c>
/// row (its columns are nullable since it is absent until Completed); its per-test
/// <see cref="TestCaseResult"/> collection maps to its own
/// <c>submission_test_results</c> table with a composite PK (submission_id, ordinal).
/// Field-access mode lets EF write to the private backing field / read-only props.
/// </summary>
public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("submissions");

        builder.HasKey(s => s.Id);

        // DomainEvents is not a persistent property — ignore it so EF does not
        // attempt to map the IReadOnlyCollection<IDomainEvent> navigation.
        builder.Ignore(s => s.DomainEvents);

        builder.Property(s => s.Id)
            .HasColumnName("submission_id")
            .HasConversion(id => id.Value, value => SubmissionId.From(value))
            .ValueGeneratedNever();

        // Cross-context references — value only, no DB FK.
        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();

        builder.Property(s => s.ProblemId)
            .HasColumnName("problem_id")
            .HasConversion(id => id.Value, value => ProblemId.From(value))
            .IsRequired();

        builder.Property(s => s.Language)
            .HasColumnName("language")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.SourceCode)
            .HasColumnName("source_code")
            .HasConversion(c => c.Value, value => SourceCode.Create(value).Value)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.SubmittedAt)
            .HasColumnName("submitted_at")
            .IsRequired();

        builder.Property(s => s.FailureReason)
            .HasColumnName("failure_reason");

        // Index supporting "list a user's submissions, newest first" and the
        // first-accepted-solve lookup.
        builder.HasIndex(s => new { s.UserId, s.SubmittedAt });

        // Evaluation — table-split into the submissions row. Columns are nullable
        // because the evaluation is absent until the submission is Completed.
        builder.OwnsOne(s => s.Evaluation, eval =>
        {
            eval.Property(e => e.Verdict)
                .HasColumnName("verdict")
                .HasConversion<string>()
                .HasMaxLength(30);

            eval.Property(e => e.PointsAwarded).HasColumnName("points_awarded");
            eval.Property(e => e.RuntimeMs).HasColumnName("runtime_ms");
            eval.Property(e => e.MemoryKb).HasColumnName("memory_kb");
            eval.Property(e => e.EvaluatedAt).HasColumnName("evaluated_at");

            // Per-test results — owned-within-owned, mapped to its own table.
            eval.OwnsMany(e => e.Results, results =>
            {
                results.ToTable("submission_test_results");
                results.WithOwner().HasForeignKey("submission_id");
                results.HasKey("submission_id", nameof(TestCaseResult.Ordinal));

                results.Property(r => r.Ordinal).HasColumnName("ordinal").ValueGeneratedNever();
                results.Property(r => r.Status)
                    .HasColumnName("status")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                results.Property(r => r.RuntimeMs).HasColumnName("runtime_ms");
                results.Property(r => r.MemoryKb).HasColumnName("memory_kb");
                results.Property(r => r.IsHidden).HasColumnName("is_hidden");
                results.Property(r => r.ActualOutput).HasColumnName("actual_output");
            });

            // Read directly into the private backing field on materialization.
            eval.Navigation(e => e.Results).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // Evaluation has a private setter on the aggregate — use field access.
        builder.Navigation(s => s.Evaluation).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
