using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos.Habits;

/// <summary>
/// Query parameters for filtering habits in GET /api/habits endpoint.
/// </summary>
/// <param name="Search">Optional search term to filter habits by name or description.</param>
/// <param name="Type">Optional habit type to filter habits.</param>
/// <param name="Status">Optional habit status to filter habits.</param>
/// <param name="Sort">Optional multi-column sort expression, e.g. status:asc,createdAtUtc:desc.</param>
/// <param name="PageNumber">Optional page number to navigate through the results (must be >= 1).</param>
/// <param name="PageSize">Optional page size to limit the number of results per page (must be between 1 and 100).</param>
public record HabitsQueryParameters(
    string? Search = null,
    HabitType? Type = null,
    HabitStatus? Status = null,
    string? Sort = null,
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    int PageNumber = 1,
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    int PageSize = 10);
