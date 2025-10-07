using System.Linq.Expressions;

namespace eShop.Application.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies the predicate only if the condition is true.
    /// </summary>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}
