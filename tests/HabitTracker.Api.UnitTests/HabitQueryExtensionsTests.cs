using HabitTracker.Api.Dtos.Habits;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Extensions;

namespace HabitTracker.Api.UnitTests;

public sealed class HabitQueryExtensionsTests
{
    [Fact]
    public void ApplySortingWhenSortIsNullUsesDefaultOrdering()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habits = new[]
        {
            Habit.Create(userId, "Beta"),
            Habit.Create(userId, "Alpha"),
            Habit.Create(userId, "Gamma"),
        }.AsQueryable();

        // Act
        var result = habits.ApplySorting(null).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplySortingWhenSortByNameAscSortsAlphabetically()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habits = new[]
        {
            Habit.Create(userId, "Charlie"),
            Habit.Create(userId, "Alpha"),
            Habit.Create(userId, "Bravo"),
        }.AsQueryable();

        // Act
        var result = habits.ApplySorting("name:asc").ToList();

        // Assert
        Assert.Collection(
            result,
            habit => Assert.Equal("Alpha", habit.Name),
            habit => Assert.Equal("Bravo", habit.Name),
            habit => Assert.Equal("Charlie", habit.Name));
    }

    [Fact]
    public void ApplySortingWhenSortByStatusThenNameAppliesMultiColumnOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var a = Habit.Create(userId, "A", type: HabitType.Binary);
        a.Archive();
        var b = Habit.Create(userId, "B", type: HabitType.Binary);
        var c = Habit.Create(userId, "C", type: HabitType.Binary);
        c.Archive();

        var habits = new[] { c, b, a }.AsQueryable();

        // Act
        var result = habits.ApplySorting("status:asc,name:desc").ToList();

        // Assert
        Assert.Collection(
            result,
            habit => Assert.Equal("B", habit.Name),
            habit => Assert.Equal("C", habit.Name),
            habit => Assert.Equal("A", habit.Name));
    }

    [Theory]
    [InlineData("unknown:asc")]
    [InlineData("name:sideways")]
    [InlineData(":asc")]
    public void ApplySortingWhenSortIsInvalidThrowsArgumentException(string sort)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habits = new[]
        {
            Habit.Create(userId, "Alpha"),
        }.AsQueryable();

        // Act + Assert
        Assert.Throws<ArgumentException>(() => habits.ApplySorting(sort).ToList());
    }

    [Fact]
    public void ApplyFilteringWhenQueryParametersNullReturnsOriginalSet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habits = new[]
        {
            Habit.Create(userId, "Read"),
            Habit.Create(userId, "Run"),
        }.AsQueryable();

        // Act
        var result = habits.ApplyFiltering(null).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplyFilteringWhenSearchTypeAndStatusProvidedReturnsMatchingHabits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var read = Habit.Create(userId, "Read book", type: HabitType.Measurable, description: "Read 20 pages");
        var run = Habit.Create(userId, "Run", type: HabitType.Binary, description: "Run 5km");
        var doneRead = Habit.Create(userId, "Read summary", type: HabitType.Measurable, description: "Read notes");
        doneRead.Archive();

        var habits = new[] { read, run, doneRead }.AsQueryable();
        var parameters = new HabitsQueryParameters(
            Search: "Read",
            Type: HabitType.Measurable,
            Status: HabitStatus.Completed);

        // Act
        var result = habits.ApplyFiltering(parameters).ToList();

        // Assert
        var single = Assert.Single(result);
        Assert.Equal("Read summary", single.Name);
    }

    [Fact]
    public void ApplyPaginationWhenInputsValidReturnsExpectedPageSlice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habits = new[]
        {
            Habit.Create(userId, "A"),
            Habit.Create(userId, "B"),
            Habit.Create(userId, "C"),
            Habit.Create(userId, "D"),
            Habit.Create(userId, "E"),
        }.AsQueryable();

        var parameters = new HabitsQueryParameters(PageNumber: 2, PageSize: 2);

        // Act
        var result = habits.ApplyPagination(parameters).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Collection(
            result,
            habit => Assert.Equal("C", habit.Name),
            habit => Assert.Equal("D", habit.Name));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    public void ApplyPaginationWhenInputsInvalidThrowsArgumentException(int pageNumber, int pageSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var habits = new[]
        {
            Habit.Create(userId, "A"),
        }.AsQueryable();

        var parameters = new HabitsQueryParameters(PageNumber: pageNumber, PageSize: pageSize);

        // Act + Assert
        Assert.Throws<ArgumentException>(() => habits.ApplyPagination(parameters).ToList());
    }
}
