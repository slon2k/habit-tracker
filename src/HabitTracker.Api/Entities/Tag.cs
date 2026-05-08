namespace HabitTracker.Api.Entities;

/// <summary>
/// Represents a user-defined tag for organizing and categorizing habits.
/// Tags are user-scoped; each user has their own tag namespace with unique names.
/// </summary>
public sealed class Tag
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>
    /// Tag name, max 50 characters. Unique per user.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    // Navigation property for many-to-many relationship
    public ICollection<HabitTag> HabitTags { get; private set; } = new List<HabitTag>();

    // Constructor for EF
    private Tag()
    {
    }

    /// <summary>
    /// Creates a new tag with a client-generated GUID.
    /// </summary>
    public Tag(Guid id, Guid userId, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, 50);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        UserId = userId;
        Name = name;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the tag name.
    /// </summary>
    public void UpdateName(string newName)
    {
        ArgumentNullException.ThrowIfNull(newName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(newName.Length, 50);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        Name = newName;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
