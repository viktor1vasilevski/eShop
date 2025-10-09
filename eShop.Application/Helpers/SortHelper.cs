using eShop.Domain.Models;

namespace eShop.Application.Helpers;

public static class SortHelper
{
    public static Func<IQueryable<Category>, IOrderedQueryable<Category>>? BuildCategorySort(
        string? sortBy, string? direction)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || string.IsNullOrWhiteSpace(direction))
            return null;

        bool asc = direction.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return sortBy.Trim().ToLower() switch
        {
            "created" => asc
                ? q => q.OrderBy(x => x.Created)
                : q => q.OrderByDescending(x => x.Created),
            "lastmodified" => asc
                ? q => q.OrderBy(x => x.LastModified)
                : q => q.OrderByDescending(x => x.LastModified),
            _ => asc
                ? q => q.OrderBy(x => x.Created)
                : q => q.OrderByDescending(x => x.Created)
        };
    }
}
