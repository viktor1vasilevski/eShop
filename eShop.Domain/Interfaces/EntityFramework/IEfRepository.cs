using System.Linq.Expressions;

namespace eShop.Domain.Interfaces.EntityFramework;

public interface IEfRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<(IEnumerable<TResult> Items, int TotalCount)> QueryAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryBuilder = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeBuilder = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    Task<TResult?> GetSingleAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeBuilder = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);
}
