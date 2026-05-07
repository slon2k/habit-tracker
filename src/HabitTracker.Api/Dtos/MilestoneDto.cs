namespace HabitTracker.Api.Dtos;

/// <summary>
/// Represents progress towards a milestone.
/// </summary>
public class MilestoneDto
{
    /// <summary>
    /// The target milestone value (e.g., "read 10 books").
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// Current progress towards the milestone.
    /// </summary>
    public int Current { get; set; }
}
