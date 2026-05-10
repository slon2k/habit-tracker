namespace HabitTracker.Api.Dtos.Auth;

public sealed record LoginResultDto(string AccessToken, DateTime ExpiresAtUtc);