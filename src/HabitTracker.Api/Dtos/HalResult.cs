namespace HabitTracker.Api.Dtos;

/// <summary>
/// Generic wrapper for HAL+JSON API responses, containing data and HATEOAS links.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public record HalResult<T>(
    T Data,
    IReadOnlyList<HateoasLink> Links);