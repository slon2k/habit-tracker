namespace HabitTracker.Api.Dtos;

using HabitTracker.Api.Entities;

/// <summary>
/// Represents the full details of a single habit, including associated tags.
/// Returned only by GET /api/habits/{habitId}. All other habit endpoints use <see cref="HabitDto"/>.
/// </summary>
public record HabitDetailsDto(
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
    DateTime? LastCompletedAtUtc,
    IReadOnlyList<TagDto> Tags)
{
    /// <summary>
    /// Maps a domain Habit entity and its resolved tags to a details DTO.
    /// Tags must be pre-loaded and user-scoped by the caller.
    /// </summary>
    public static HabitDetailsDto FromEntity(Habit habit, IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(habit);
        ArgumentNullException.ThrowIfNull(tags);

        return new HabitDetailsDto(
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
            LastCompletedAtUtc: habit.LastCompletedAtUtc,
            Tags: tags.Select(TagDto.FromEntity).ToList());
    }
}
