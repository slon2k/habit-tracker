using HabitTracker.Api.Entities;

using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<HabitTag> HabitTags => Set<HabitTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schemas.Application);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
