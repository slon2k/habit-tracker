namespace HabitTracker.Api.Entities;

/// <summary>
/// Join entity representing the many-to-many relationship between habits and tags.
/// </summary>
public sealed class HabitTag
{
    public Guid HabitId { get; private set; }

    public Guid TagId { get; private set; }

    // Navigation properties
    public Habit Habit { get; private set; } = null!;

    public Tag Tag { get; private set; } = null!;

    // Constructor for EF
    private HabitTag()
    {
    }

    /// <summary>
    /// Creates a new habit-tag association.
    /// </summary>
    public HabitTag(Guid habitId, Guid tagId)
    {
        HabitId = habitId;
        TagId = tagId;
    }
}
