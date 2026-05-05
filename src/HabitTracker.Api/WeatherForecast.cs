
using System.Diagnostics.CodeAnalysis;
namespace HabitTracker.Api;

[SuppressMessage("Design", "CA1515:Public types in internal assemblies should be internal", Justification = "Instantiated by ASP.NET Core framework")]
public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
