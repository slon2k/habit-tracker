using HabitTracker.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HabitTracker.Api.Extensions;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        int pageNumber,
        int pageSize,
        Func<TSource, TResult> selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(selector);

        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ArgumentException("PageNumber and PageSize must be greater than 0.");
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TResult>(
            items.Select(selector).ToList(),
            totalCount,
            pageNumber,
            pageSize);
    }

    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> query,
        int pageNumber,
        int pageSize,
        Expression<Func<TSource, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(selector);

        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ArgumentException("PageNumber and PageSize must be greater than 0.");
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PagedResult<TResult>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }
}