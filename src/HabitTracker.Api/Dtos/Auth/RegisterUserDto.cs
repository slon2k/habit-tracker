using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.Dtos.Auth;

public sealed record RegisterUserDto(
    [Required][EmailAddress] string Email,
    [Required] string Name,
    [Required][MinLength(8)] string Password,
    [Required] string ConfirmPassword) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Password != ConfirmPassword)
        {
            yield return new ValidationResult("Passwords do not match.", [nameof(ConfirmPassword)]);
        }
    }
}