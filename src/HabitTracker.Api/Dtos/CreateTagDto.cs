namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request DTO for creating a new tag.
/// </summary>
public record CreateTagDto(string Name)
{
    public CreateTagDto() : this(string.Empty)
    {
    }
}
