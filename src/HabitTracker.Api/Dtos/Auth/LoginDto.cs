using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.Dtos.Auth;

public sealed record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password);