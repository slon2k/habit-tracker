namespace HabitTracker.Api.Dtos;

/// <summary>
/// Represents the frequency of a habit.
/// </summary>
public record FrequencyDto(
    string Type,
    int TimesPerPeriod);
