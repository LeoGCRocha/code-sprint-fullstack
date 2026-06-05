using System.Text.Json;
using CodeSprint.Problems.Domain;
using CodeSprint.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Problems.Infrastructure;

/// <summary>
/// EF Core mapping for the <see cref="Problem"/> aggregate.
///
/// Strongly-typed id and value objects are mapped with value converters (single
/// columns). The backing-field collections (_notes, _inputFormat, _constraints)
/// are stored as jsonb. Owned collections (Examples, Tags, TestCases) map to
/// separate tables; field-access mode lets EF write directly to the backing fields
/// so the IReadOnlyList properties remain read-only.
/// </summary>
public sealed class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
    private static readonly JsonSerializerOptions JsonOpts = new();

    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        builder.ToTable("problems");

        builder.HasKey(p => p.Id);

        // DomainEvents is not a persistent property — ignore it so EF does not
        // attempt to map the IReadOnlyCollection<IDomainEvent> navigation.
        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.Id)
            .HasColumnName("problem_id")
            .HasConversion(id => id.Value, value => ProblemId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasConversion(s => s.Value, value => Slug.Create(value).Value)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Difficulty)
            .HasColumnName("difficulty")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Points)
            .HasColumnName("points")
            .IsRequired();

        builder.Property(p => p.EstimatedMinutes)
            .HasColumnName("estimated_minutes")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(p => p.IsPublished)
            .HasColumnName("is_published")
            .IsRequired();

        // jsonb columns for list-of-string fields — accessed via backing fields
        builder.Property<List<string>>("_notes")
            .HasColumnName("notes")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts)!)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<List<string>>("_inputFormat")
            .HasColumnName("input_format")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts)!)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<List<string>>("_constraints")
            .HasColumnName("constraints")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts)!)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Owned collections — separate tables, FK back to the Problem row.
        // Navigation.UsePropertyAccessMode(Field) causes EF to populate the
        // backing fields (_examples, _tags, _testCases) directly on load.
        builder.OwnsMany(p => p.Examples, examples =>
        {
            examples.ToTable("problem_examples");
            examples.WithOwner().HasForeignKey("problem_id");
            examples.HasKey("problem_id", nameof(Example.Ordinal));
            examples.Property(e => e.Ordinal).HasColumnName("ordinal").ValueGeneratedNever();
            examples.Property(e => e.Input).HasColumnName("input").IsRequired();
            examples.Property(e => e.Output).HasColumnName("output").IsRequired();
            examples.Property(e => e.Explanation).HasColumnName("explanation");
        });
        builder.Navigation(p => p.Examples)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.Tags, tags =>
        {
            tags.ToTable("problem_tags");
            tags.WithOwner().HasForeignKey("problem_id");
            tags.HasKey("problem_id", nameof(Tag.Value));
            tags.Property(t => t.Value)
                .HasColumnName("tag")
                .HasMaxLength(50)
                .IsRequired();
        });
        builder.Navigation(p => p.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.TestCases, testCases =>
        {
            testCases.ToTable("problem_test_cases");
            testCases.WithOwner().HasForeignKey("problem_id");
            testCases.HasKey("problem_id", nameof(TestCase.Ordinal));
            testCases.Property(tc => tc.Ordinal).HasColumnName("ordinal").ValueGeneratedNever();
            testCases.Property(tc => tc.Input).HasColumnName("input").IsRequired();
            testCases.Property(tc => tc.ExpectedOutput).HasColumnName("expected_output").IsRequired();
            testCases.Property(tc => tc.IsHidden).HasColumnName("is_hidden").IsRequired();
        });
        builder.Navigation(p => p.TestCases)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
