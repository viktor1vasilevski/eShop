using eShop.Domain.Interfaces.Base;
using System.Linq.Expressions;

namespace eShop.Domain.Interfaces;

public interface IEfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    Task<(IEnumerable<TResult> Items, int TotalCount)> QueryAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryBuilder = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeBuilder = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);

    Task<TResult?> GetSingleAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeBuilder = null,
        CancellationToken cancellationToken = default);
}

