namespace HabitTracker.Api.Entities;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppRole
{
    Member,
    Admin,
}