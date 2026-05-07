namespace HabitTracker.Api.Dtos;

/// <summary>
/// Represents the frequency of a habit.
/// </summary>
public class FrequencyDto
{
    /// <summary>
    /// The frequency type (Daily, Weekly, Monthly, Yearly).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// How many times per period the habit should be performed.
    /// </summary>
    public int TimesPerPeriod { get; set; }
}
