using HabitTracker.Api.Dtos.Habits;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.UnitTests;

public sealed class HabitDtoMappingTests
{
    [Fact]
    public void CreateHabitDto_ToHabit_MapsAllFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var endDate = new DateOnly(2026, 12, 31);
        var reminderTime = new TimeOnly(9, 15);

        var dto = new CreateHabitDto(
            Name: "Read books",
            Description: "Read 20 pages",
            Type: "Measurable",
            Frequency: new FrequencyCreateDto(Type: "Weekly", TimesPerPeriod: 3),
            Target: new TargetCreateDto(Value: 20, Unit: "pages"),
            EndDate: endDate,
            ReminderTime: reminderTime);

        // Act
        var habit = dto.ToHabit(userId);

        // Assert
        Assert.Equal(userId, habit.UserId);
        Assert.Equal("Read books", habit.Name);
        Assert.Equal("Read 20 pages", habit.Description);
        Assert.Equal(HabitType.Measurable, habit.Type);
        Assert.Equal(FrequencyType.Weekly, habit.Frequency.Type);
        Assert.Equal(3, habit.Frequency.TimesPerPeriod);
        Assert.Equal(20, habit.Target.Value);
        Assert.Equal("pages", habit.Target.Unit);
        Assert.Equal(endDate, habit.EndDate);
        Assert.Equal(reminderTime, habit.ReminderTime);
    }

    [Fact]
    public void CreateHabitDto_ToHabit_WhenTimesPerPeriodNull_UsesDefaultValueOne()
    {
        // Arrange
        var dto = new CreateHabitDto(
            Name: "Walk",
            Description: null,
            Type: "Binary",
            Frequency: new FrequencyCreateDto(Type: "Daily", TimesPerPeriod: null),
            Target: null,
            EndDate: null,
            ReminderTime: null);

        // Act
        var habit = dto.ToHabit(Guid.NewGuid());

        // Assert
        Assert.Equal(1, habit.Frequency.TimesPerPeriod);
        Assert.Null(habit.Target.Value);
        Assert.Null(habit.Target.Unit);
    }

    [Fact]
    public void HabitDto_FromEntity_MapsExpectedFields()
    {
        // Arrange
        var habit = Habit.Create(
            userId: Guid.NewGuid(),
            name: "Run",
            description: "Run 5km",
            type: HabitType.Binary,
            frequency: new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
            target: new Target { Value = 5, Unit = "km" },
            endDate: null,
            reminderTime: null);

        // Act
        var dto = HabitDto.FromEntity(habit);

        // Assert
        Assert.Equal(habit.Id, dto.Id);
        Assert.Equal("Run", dto.Name);
        Assert.Equal("Binary", dto.Type);
        Assert.Equal("Ongoing", dto.Status);
        Assert.Equal("Daily", dto.Frequency.Type);
        Assert.Equal(1, dto.Frequency.TimesPerPeriod);
        Assert.Equal(5, dto.Target.Value);
        Assert.Equal("km", dto.Target.Unit);
    }

    [Fact]
    public void HabitDetailsDto_FromEntity_MapsTags()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habit = Habit.Create(userId, "Meditate");
        var tags = new List<Tag>
        {
            new(Guid.NewGuid(), userId, "health"),
            new(Guid.NewGuid(), userId, "mindfulness")
        };

        // Act
        var details = HabitDetailsDto.FromEntity(habit, tags);

        // Assert
        Assert.Equal(2, details.Tags.Count);
        Assert.Equal("health", details.Tags[0].Name);
        Assert.Equal("mindfulness", details.Tags[1].Name);
    }

    [Fact]
    public void UpdateHabitDto_ApplyToHabit_ReplacesAllProvidedFields()
    {
        // Arrange
        var habit = Habit.Create(
            userId: Guid.NewGuid(),
            name: "Read",
            description: "Old",
            type: HabitType.Binary,
            frequency: new Frequency { Type = FrequencyType.Daily, TimesPerPeriod = 1 },
            target: new Target { Value = 1, Unit = "book" },
            endDate: null,
            reminderTime: null);

        var dto = new UpdateHabitDto(
            Name: "Read deeply",
            Description: "New",
            Frequency: new FrequencyUpdateDto(Type: "Monthly", TimesPerPeriod: 2),
            Target: new TargetUpdateDto(Value: 50, Unit: "pages"),
            EndDate: new DateOnly(2026, 10, 1),
            ReminderTime: new TimeOnly(8, 0),
            Milestone: new UpdateMilestoneDto(Target: 5));

        // Act
        dto.ApplyToHabit(habit);

        // Assert
        Assert.Equal("Read deeply", habit.Name);
        Assert.Equal("New", habit.Description);
        Assert.Equal(FrequencyType.Monthly, habit.Frequency.Type);
        Assert.Equal(2, habit.Frequency.TimesPerPeriod);
        Assert.Equal(50, habit.Target.Value);
        Assert.Equal("pages", habit.Target.Unit);
        Assert.Equal(new DateOnly(2026, 10, 1), habit.EndDate);
        Assert.Equal(new TimeOnly(8, 0), habit.ReminderTime);
        Assert.Equal(5, habit.Milestone.Target);
    }

    [Fact]
    public void PartialUpdateHabitDto_ApplyToHabit_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var habit = Habit.Create(
            userId: Guid.NewGuid(),
            name: "Workout",
            description: "Initial",
            type: HabitType.Measurable,
            frequency: new Frequency { Type = FrequencyType.Weekly, TimesPerPeriod = 4 },
            target: new Target { Value = 30, Unit = "minutes" },
            endDate: new DateOnly(2026, 11, 30),
            reminderTime: new TimeOnly(7, 30));

        var dto = new PartialUpdateHabitDto(
            Name: null,
            Description: "Updated description",
            Frequency: new FrequencyPartialUpdateDto(Type: null, TimesPerPeriod: 2),
            Target: new TargetPartialUpdateDto(Value: null, Unit: "mins"),
            EndDate: null,
            ReminderTime: null,
            Milestone: null);

        // Act
        dto.ApplyToHabit(habit);

        // Assert
        Assert.Equal("Workout", habit.Name);
        Assert.Equal("Updated description", habit.Description);
        Assert.Equal(FrequencyType.Weekly, habit.Frequency.Type);
        Assert.Equal(2, habit.Frequency.TimesPerPeriod);
        Assert.Equal(30, habit.Target.Value);
        Assert.Equal("mins", habit.Target.Unit);
        Assert.Equal(new DateOnly(2026, 11, 30), habit.EndDate);
        Assert.Equal(new TimeOnly(7, 30), habit.ReminderTime);
    }
}
