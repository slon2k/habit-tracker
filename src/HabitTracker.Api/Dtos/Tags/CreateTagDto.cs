namespace HabitTracker.Api.Dtos.Tags;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for creating a new tag.
/// </summary>
/// <summary>
/// Request DTO for creating a new tag.
/// </summary>
/// <param name="Name">Tag name (1-50 chars, required).</param>
public record CreateTagDto(
    [Required(ErrorMessage = "Tag name is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag name must be between 1 and 50 characters.")]
    string Name);
