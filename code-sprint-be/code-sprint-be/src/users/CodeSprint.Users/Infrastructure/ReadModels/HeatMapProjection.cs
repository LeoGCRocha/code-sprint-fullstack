
using CodeSprint.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Users.ReadModels;

/// <summary>
/// Read-model row: one (user, day) activity bucket feeding the profile heatmap.
/// Projected from SubmissionEvaluatedV1 — NOT a domain aggregate. Carries no domain
/// events and owns no invariant beyond a monotonic count. Rebuildable by replay.
/// </summary>
public class HeatmapDay
{
    private HeatmapDay() { } // EF materialization

    public HeatmapDay(UserId userId, DateOnly day)
    {
        UserId = userId;
        Day = day;
        Count = 1;
    }

    public UserId UserId { get; private set; }
    public DateOnly Day { get; private set; }
    public int Count { get; private set; }

    public void Increment() => Count++;
}

public sealed class HeatmapDayConfiguration : IEntityTypeConfiguration<HeatmapDay>
{
    public void Configure(EntityTypeBuilder<HeatmapDay> builder)
    {
        builder.ToTable("heatmap_days");

        // Composite PK. Order (user_id, day) = prefix scan for "all days of a user".
        builder.HasKey(h => new { h.UserId, h.Day });

        builder.Property(h => h.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => UserId.From(value));

        builder.Property(h => h.Day)
            .HasColumnName("day")
            .HasColumnType("date");

        builder.Property(h => h.Count)
            .HasColumnName("count")
            .IsRequired();
    }
}