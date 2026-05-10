namespace HabitTracker.Api.Dtos;

/// <summary>
/// Paginated result with HATEOAS links for navigation and actions.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <param name="Data">The list of items for the current page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="PageNumber">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Links">HATEOAS links for navigation and actions.</param>
public record HalPagedResult<T>(
    IReadOnlyList<T> Data,
    int TotalCount,
    int PageNumber,
    int PageSize,
    IReadOnlyList<HateoasLink> Links) : HalResult<IReadOnlyList<T>>(Data, Links)
{
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Gets a value indicating whether there are more pages after the current page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Gets a value indicating whether there are pages before the current page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
