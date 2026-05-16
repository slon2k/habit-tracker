namespace HabitTracker.Api.Dtos.Common;

/// <summary>
/// Represents a hyperlink in HAL+JSON format.
/// </summary>
/// <param name="Href">The target URI of the link.</param>
/// <param name="Rel">The link relation type (e.g., "self", "next", "prev").</param>
/// <param name="Method">Optional HTTP method (e.g., "GET", "POST", "PUT", "DELETE").</param>
/// <param name="Title">Optional human-readable title of the link.</param>
public record HateoasLink(
    string Href,
    string Rel,
    string? Method = "GET",
    string? Title = null);
