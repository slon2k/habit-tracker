
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("[controller]")]
[SuppressMessage("Design", "CA1515:Public types in internal assemblies should be internal", Justification = "Instantiated by ASP.NET Core framework")]
public class WeatherForecastController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("HabitTracker.Api");

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using Activity? activity = ActivitySource.StartActivity("WeatherForecast.Generate", ActivityKind.Internal);
        activity?.SetTag("weatherforecast.count", 5);

        return [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
            Summary = Summaries[RandomNumberGenerator.GetInt32(Summaries.Length)]
        })];
    }
}
