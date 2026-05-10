namespace HabitTracker.Api.Services.Auth;

public interface ITokenService
{
    JwtTokenResult CreateAccessToken(string identityUserId, Guid appUserId, string email);
}
