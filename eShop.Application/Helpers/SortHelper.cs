using eShop.Domain.Models;
using eShop.Domain.Models.Base;

namespace eShop.Application.Helpers;

public static class SortHelper
{
    /// <summary>
    /// Builds a dynamic OrderBy function for any entity type with Created/LastModified properties.
    /// </summary>
    public static Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? BuildSort<TEntity>(
        string? sortBy,
        string? direction)
        where TEntity : AuditableBaseEntity
    {
        if (string.IsNullOrWhiteSpace(sortBy) || string.IsNullOrWhiteSpace(direction))
            return null;

        bool asc = direction.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return sortBy.Trim().ToLower() switch
        {
            "created" => asc
                ? q => q.OrderBy(e => e.Created)
                : q => q.OrderByDescending(e => e.Created),

            "lastmodified" => asc
                ? q => q.OrderBy(e => e.LastModified)
                : q => q.OrderByDescending(e => e.LastModified),

            _ => asc
                ? q => q.OrderBy(e => e.Created)
                : q => q.OrderByDescending(e => e.Created)
        };
    }
}
