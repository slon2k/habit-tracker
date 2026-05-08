namespace HabitTracker.Api.Dtos;

/// <summary>
/// Represents progress towards a milestone.
/// </summary>
public record MilestoneDto(
    int Target,
    int Current);
