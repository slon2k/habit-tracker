namespace HabitTracker.Api.Dtos;

using HabitTracker.Api.Entities;

/// <summary>
/// Represents a habit in API responses. Excludes internal tracking fields and UserId for security.
/// </summary>
public class HabitDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public DateOnly? EndDate { get; set; }

    public TimeOnly? ReminderTime { get; set; }

    public FrequencyDto Frequency { get; set; } = new();

    public TargetDto Target { get; set; } = new();

    public MilestoneDto Milestone { get; set; } = new();

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastCompletedAtUtc { get; set; }

    /// <summary>
    /// Maps a domain Habit entity to a DTO for API response.
    /// Excludes UserId, ArchivedAt, LastStreakBrokenAt, and UpdatedAtUtc for security and simplicity.
    /// </summary>
    public static HabitDto FromEntity(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);

        return new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type.ToString(),
            Status = habit.Status.ToString(),
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,
            ReminderTime = habit.ReminderTime,
            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type.ToString(),
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },
            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },
            Milestone = new MilestoneDto
            {
                Target = habit.Milestone.Target,
                Current = habit.Milestone.Current
            },
            CurrentStreak = habit.CurrentStreak,
            LongestStreak = habit.LongestStreak,
            CreatedAtUtc = habit.CreatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }
}
