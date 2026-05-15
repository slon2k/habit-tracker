using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.Dtos.Auth;

public sealed record RefreshTokenDto(
    [Required] string AccessToken,
    [Required] string RefreshToken);