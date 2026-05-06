using HabitTracker.Api.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("habits");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(2000);

        builder.Property(h => h.Type)
            .IsRequired();

        builder.Property(h => h.Status)
            .IsRequired();

        builder.OwnsOne(h => h.Frequency, frequency =>
        {
            frequency.Property(f => f.Type)
                .HasColumnName("frequency_type")
                .IsRequired();

            frequency.Property(f => f.TimesPerPeriod)
                .HasColumnName("frequency_times_per_period")
                .IsRequired();
        });

        builder.OwnsOne(h => h.Target, target =>
        {
            target.Property(t => t.Value)
                .HasColumnName("target_value");

            target.Property(t => t.Unit)
                .HasColumnName("target_unit")
                .HasMaxLength(100);
        });

        builder.OwnsOne(h => h.Milestone, milestone =>
        {
            milestone.Property(m => m.Target)
                .HasColumnName("milestone_target");

            milestone.Property(m => m.Current)
                .HasColumnName("milestone_current");
        });

        builder.Property(h => h.CreatedAtUtc)
            .IsRequired();
    }
}