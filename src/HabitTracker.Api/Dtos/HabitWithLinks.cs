namespace HabitTracker.Api.Dtos;

/// <summary>
/// A habit detail response with HATEOAS links.
/// </summary>
/// <param name="Data">The habit details.</param>
/// <param name="Links">HATEOAS links for navigation and actions.</param>
public record HabitWithLinks(
    HabitDetailsDto Data,
    IReadOnlyList<HateoasLink> Links);

/// <summary>
/// A created habit response with HATEOAS links.
/// </summary>
/// <param name="Data">The habit DTO.</param>
/// <param name="Links">HATEOAS links for navigation and actions.</param>
public record HabitCreatedWithLinks(
    HabitDto Data,
    IReadOnlyList<HateoasLink> Links);
