using HabitTracker.Api.Data;

using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("habits")]
public class HabitsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public HabitsController(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    // GET /habits
    [HttpGet]
    public IActionResult GetHabits()
    {
        var habits = _dbContext.Habits.ToList();
        return Ok(habits);
    }
}