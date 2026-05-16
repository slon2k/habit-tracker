namespace HabitTracker.Api.Dtos.Habits;

using HabitTracker.Api.Dtos.Models;
using HabitTracker.Api.Dtos.Tags;
using HabitTracker.Api.Entities;

/// <summary>
/// Represents a habit in API responses. Excludes internal tracking fields and UserId for security.
/// </summary>
public record HabitDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string Status,
    bool IsArchived,
    DateOnly? EndDate,
    TimeOnly? ReminderTime,
    FrequencyDto Frequency,
    TargetDto Target,
    MilestoneDto Milestone,
    int CurrentStreak,
    int LongestStreak,
    DateTime CreatedAtUtc,
    DateTime? LastCompletedAtUtc)
{
    /// <summary>
    /// Maps a domain Habit entity to a DTO for API response.
    /// Excludes UserId, ArchivedAt, LastStreakBrokenAt, and UpdatedAtUtc for security and simplicity.
    /// </summary>
    public static HabitDto FromEntity(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);

        return new HabitDto(
            Id: habit.Id,
            Name: habit.Name,
            Description: habit.Description,
            Type: habit.Type.ToString(),
            Status: habit.Status.ToString(),
            IsArchived: habit.IsArchived,
            EndDate: habit.EndDate,
            ReminderTime: habit.ReminderTime,
            Frequency: new FrequencyDto(
                Type: habit.Frequency.Type.ToString(),
                TimesPerPeriod: habit.Frequency.TimesPerPeriod),
            Target: new TargetDto(
                Value: habit.Target.Value,
                Unit: habit.Target.Unit),
            Milestone: new MilestoneDto(
                Target: habit.Milestone.Target,
                Current: habit.Milestone.Current),
            CurrentStreak: habit.CurrentStreak,
            LongestStreak: habit.LongestStreak,
            CreatedAtUtc: habit.CreatedAtUtc,
            LastCompletedAtUtc: habit.LastCompletedAtUtc);
    }
}
