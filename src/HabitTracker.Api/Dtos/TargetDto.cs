namespace HabitTracker.Api.Dtos;

/// <summary>
/// Represents a target value and unit for measurable habits.
/// </summary>
public record TargetDto(
    int? Value,
    string? Unit);
