namespace HabitTracker.Api.Entities;

/// <summary>
/// Represents a habit that a user wants to track and build.
/// Habits can be binary (done/not done) or measurable (tracked with a target value).
/// </summary>
public sealed class Habit
{
    // Properties

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public HabitType Type { get; private set; }

    public Frequency Frequency { get; private set; }

    public Target Target { get; private set; }

    public HabitStatus Status { get; private set; }

    public bool IsArchived { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public Milestone Milestone { get; private set; }

    public TimeOnly? ReminderTime { get; private set; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }

    public DateTime? LastStreakBrokenAt { get; set; }

    public DateTime? ArchivedAt { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public DateTime? LastCompletedAtUtc { get; private set; }

    // Navigation property for many-to-many relationship
    public ICollection<HabitTag> HabitTags { get; private set; } = new List<HabitTag>();

    // Constructors

    private Habit() 
    { 
        Frequency = new Frequency();
        Target = new Target();
        Milestone = new Milestone();
    }

    // Factory Methods

    /// <summary>
    /// Creates a new habit with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for this habit. If not provided, a new Guid is generated.</param>
    /// <param name="userId">The user who owns this habit.</param>
    /// <param name="name">The name of the habit. Must not be empty.</param>
    /// <param name="description">Optional description of the habit.</param>
    /// <param name="type">The type of habit: Binary (done/not done) or Measurable (with a target value).</param>
    /// <param name="frequency">How often the habit should be performed. Defaults to Daily, once per period.</param>
    /// <param name="target">Optional target value and unit for measurable habits.</param>
    /// <param name="endDate">Optional end date for the habit.</param>
    /// <param name="reminderTime">Optional time of day to remind the user about this habit.</param>
    /// <returns>A new Habit instance.</returns>
    /// <exception cref="ArgumentException">Thrown if name is empty, type is None, or frequency is invalid.</exception>
    public static Habit Create(
        Guid userId,
        string name,
        string? description = null,
        HabitType type = HabitType.Binary,
        Frequency? frequency = null,
        Target? target = null,
        DateOnly? endDate = null,
        TimeOnly? reminderTime = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Habit name cannot be empty.", nameof(name));

        if (type == HabitType.None)
            throw new ArgumentException("Habit type must be Binary or Measurable, not None.", nameof(type));

        frequency ??= new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 };

        if (frequency.Type == FrequencyType.None || frequency.TimesPerPeriod <= 0)
            throw new ArgumentException("Frequency type must be valid and times per period must be positive.", nameof(frequency));

        var habit = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = description,
            Type = type,
            Frequency = frequency,
            Target = target ?? new Target(),
            Milestone = new Milestone { Target = 0, Current = 0 },
            EndDate = endDate,
            ReminderTime = reminderTime,
            Status = HabitStatus.Ongoing,
            IsArchived = false,
            CreatedAtUtc = DateTime.UtcNow,
        };

        return habit;
    }

    // Domain Methods

    /// <summary>
    /// Marks this habit as completed and updates completion tracking.
    /// </summary>
    public void Complete()
    {
        LastCompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        // If milestone tracking is enabled (Target.Value is set for measurable habits)
        if (Type == HabitType.Measurable && Target.Value.HasValue && Milestone.Current < Milestone.Target)
        {
            Milestone.Current++;
        }
    }

    /// <summary>
    /// Archives this habit, marking it as no longer active.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        Status = HabitStatus.Completed;
    }

    /// <summary>
    /// Unarchives this habit, marking it as active again.
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
        ArchivedAt = null;
        UpdatedAtUtc = DateTime.UtcNow;
        Status = HabitStatus.Ongoing;
    }

    /// <summary>
    /// Resets the milestone progress to zero.
    /// </summary>
    public void ResetMilestone()
    {
        Milestone.Current = 0;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the milestone target value. Current is preserved.
    /// </summary>
    public void UpdateMilestoneTarget(int target)
    {
        if (target < 0)
            throw new ArgumentException("Milestone target must be non-negative.", nameof(target));

        Milestone.Target = target;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the habit's name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Habit name cannot be empty.", nameof(name));

        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the habit's description.
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the habit's frequency.
    /// </summary>
    public void UpdateFrequency(Frequency frequency)
    {
        ArgumentNullException.ThrowIfNull(frequency);

        if (frequency.Type == FrequencyType.None || frequency.TimesPerPeriod <= 0)
            throw new ArgumentException("Frequency type must be valid and times per period must be positive.", nameof(frequency));

        Frequency = frequency;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the habit's target.
    /// </summary>
    public void UpdateTarget(Target target)
    {
        ArgumentNullException.ThrowIfNull(target);

        Target = target;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the habit's end date.
    /// </summary>
    public void UpdateEndDate(DateOnly? endDate)
    {
        EndDate = endDate;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the habit's reminder time.
    /// </summary>
    public void UpdateReminderTime(TimeOnly? reminderTime)
    {
        ReminderTime = reminderTime;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Recomputes and updates streak information based on completion history.
    /// This method is a placeholder and will be fully implemented when the Entry entity is available.
    /// Implementation note: Query Entry records for this habit to compute CurrentStreak, LongestStreak, and LastStreakBrokenAt.
    /// </summary>
#pragma warning disable S1135 // Suppress SonarAnalyzer TODO rule
    public void UpdateStreaks()
    {
        // Placeholder for streak calculation logic
        // When Entry entity is implemented, query Entry records for this habit
        // to compute CurrentStreak, LongestStreak, and LastStreakBrokenAt
    }
#pragma warning restore S1135
}

public sealed class Frequency
{
    public FrequencyType Type { get; set; }

    public int TimesPerPeriod { get; set; }
}

public enum FrequencyType
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4
}

public enum HabitType
{
    None = 0,
    Binary = 1,
    Measurable = 2
}

public sealed class Target
{
    /// <summary>
    /// The numerical target value (e.g., number of miles, number of pages read).
    /// Only applicable for measurable habits.
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// The unit of measurement (e.g., "miles", "pages", "minutes").
    /// Only applicable for measurable habits.
    /// </summary>
    public string? Unit { get; set; }
}

public enum HabitStatus
{
    None = 0,
    Ongoing = 1,
    Completed = 2,
}

public sealed class Milestone
{
    /// <summary>
    /// The target milestone value (e.g., read 10 books).
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// The current progress towards the milestone.
    /// </summary>
    public int Current { get; set; }
}