namespace HabitTracker.Api.Dtos.Habits;

/// <summary>
/// Request DTO for upserting tags to a habit.
/// Replaces all existing tags with the provided list.
/// </summary>
public record UpsertHabitTagsRequest(IList<Guid> TagIds)
{
    public UpsertHabitTagsRequest() : this(new List<Guid>())
    {
    }
}
