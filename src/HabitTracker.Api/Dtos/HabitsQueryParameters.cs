using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Query parameters for filtering habits in GET /api/habits endpoint.
/// </summary>
/// <param name="Search">Optional search term to filter habits by name or description.</param>
/// <param name="Type">Optional habit type to filter habits.</param>
/// <param name="Status">Optional habit status to filter habits.</param>
/// <param name="Sort">Optional multi-column sort expression, e.g. status:asc,createdAtUtc:desc.</param>
public record HabitsQueryParameters(
    string? Search = null,
    HabitType? Type = null,
    HabitStatus? Status = null,
    string? Sort = null);