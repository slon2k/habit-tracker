namespace HabitTracker.Api.Dtos.Tags;

using HabitTracker.Api.Entities;

/// <summary>
/// Represents a tag in API responses. Excludes UserId for security.
/// </summary>
/// <summary>
/// Tag DTO for API responses.
/// </summary>
/// <param name="Id">Tag unique identifier.</param>
/// <param name="Name">Tag name.</param>
/// <param name="CreatedAtUtc">Tag creation date (UTC).</param>
public record TagDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc)
{
    /// <summary>
    /// Maps a domain Tag entity to a DTO for API response.
    /// </summary>
    public static TagDto FromEntity(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return new TagDto(
            Id: tag.Id,
            Name: tag.Name,
            CreatedAtUtc: tag.CreatedAtUtc);
    }
}
