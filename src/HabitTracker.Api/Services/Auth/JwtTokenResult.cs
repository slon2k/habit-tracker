namespace HabitTracker.Api.Services.Auth;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
