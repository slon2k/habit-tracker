using HabitTracker.Api.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public class HabitTagConfiguration : IEntityTypeConfiguration<HabitTag>
{
    public void Configure(EntityTypeBuilder<HabitTag> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("habit_tags");

        // Composite primary key
        builder.HasKey(ht => new { ht.HabitId, ht.TagId });

        // Foreign key to Habit with cascade delete
        builder.HasOne(ht => ht.Habit)
            .WithMany(h => h.HabitTags)
            .HasForeignKey(ht => ht.HabitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to Tag with cascade delete
        builder.HasOne(ht => ht.Tag)
            .WithMany(t => t.HabitTags)
            .HasForeignKey(ht => ht.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
