using HabitTracker.Api.Dtos;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Extensions;

public static class HabitQueryExtensions
{
    public static IQueryable<Habit> ApplySorting(this IQueryable<Habit> query, string? sort)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(sort))
        {
            return query
                .OrderByDescending(h => h.CreatedAtUtc)
                .ThenBy(h => h.Id);
        }

        IOrderedQueryable<Habit>? orderedQuery = null;

        foreach (var clause in ParseSortClauses(sort))
        {
            orderedQuery = ApplySortClause(orderedQuery ?? query, orderedQuery is not null, clause);
        }

        return orderedQuery!
            .ThenBy(h => h.Id);
    }

    private static List<HabitSortClause> ParseSortClauses(string sort)
    {
        var rawClauses = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (rawClauses.Length == 0)
        {
            throw new ArgumentException("Sort expression cannot be empty.", nameof(sort));
        }

        var clauses = new List<HabitSortClause>(rawClauses.Length);
        foreach (var rawClause in rawClauses)
        {
            var parts = rawClause.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length is < 1 or > 2 || string.IsNullOrWhiteSpace(parts[0]))
            {
                throw new ArgumentException(
                    "Invalid sort expression. Use format field:direction, e.g. status:asc,createdAtUtc:desc.",
                    nameof(sort));
            }

            var field = NormalizeField(parts[0]);
            var direction = parts.Length == 1 ? "ASC" : parts[1].ToUpperInvariant();

            clauses.Add(direction switch
            {
                "ASC" => new HabitSortClause(field, Descending: false),
                "DESC" => new HabitSortClause(field, Descending: true),
                _ => throw new ArgumentException(
                    $"Invalid sort direction '{parts[1]}'. Allowed values are 'asc' and 'desc'.",
                    nameof(sort))
            });
        }

        return clauses;
    }

    private static string NormalizeField(string field) => field.Trim().ToUpperInvariant() switch
    {
        "NAME" => "name",
        "STATUS" => "status",
        "TYPE" => "type",
        "CREATEDATUTC" => "createdatutc",
        "UPDATEDATUTC" => "updatedatutc",
        _ => throw new ArgumentException(
            $"Invalid sort field '{field}'. Allowed fields are: name, status, type, createdAtUtc, updatedAtUtc.",
            nameof(field))
    };

    private static IOrderedQueryable<Habit> ApplySortClause(
        IQueryable<Habit> query,
        bool isThenBy,
        HabitSortClause clause)
    {
        return clause.Field switch
        {
            "name" => ApplyOrder(query, isThenBy, h => h.Name, clause.Descending),
            "status" => ApplyOrder(query, isThenBy, h => h.Status, clause.Descending),
            "type" => ApplyOrder(query, isThenBy, h => h.Type, clause.Descending),
            "createdatutc" => ApplyOrder(query, isThenBy, h => h.CreatedAtUtc, clause.Descending),
            "updatedatutc" => ApplyOrder(query, isThenBy, h => h.UpdatedAtUtc, clause.Descending),
            _ => throw new ArgumentOutOfRangeException(nameof(clause))
        };
    }

    private static IOrderedQueryable<Habit> ApplyOrder<TKey>(
        IQueryable<Habit> query,
        bool isThenBy,
        System.Linq.Expressions.Expression<Func<Habit, TKey>> keySelector,
        bool descending)
    {
        if (!isThenBy)
        {
            return descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }

        var orderedQuery = (IOrderedQueryable<Habit>)query;
        return descending
            ? orderedQuery.ThenByDescending(keySelector)
            : orderedQuery.ThenBy(keySelector);
    }

    private readonly record struct HabitSortClause(string Field, bool Descending);

    public static IQueryable<Habit> ApplyFiltering(this IQueryable<Habit> query, HabitsQueryParameters? queryParameters)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (queryParameters == null)
        {
            return query;
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            query = query.Where(h => h.Name.Contains(queryParameters.Search) ||
                                         (h.Description != null && h.Description.Contains(queryParameters.Search)));
        }

        if (queryParameters.Type.HasValue)
        {
            query = query.Where(h => h.Type == queryParameters.Type.Value);
        }

        if (queryParameters.Status.HasValue)
        {
            query = query.Where(h => h.Status == queryParameters.Status.Value);
        }

        return query;
    }
}