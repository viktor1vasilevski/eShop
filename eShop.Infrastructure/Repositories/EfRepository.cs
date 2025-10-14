using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace eShop.Infrastructure.Repositories;

public class EfRepository<TEntity>(AppDbContext _context) : IEfRepository<TEntity>, IRepository<TEntity> where TEntity : class
{
    protected readonly DbSet<TEntity> _dbSet = _context.Set<TEntity>();


    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;
        if (predicate != null)
            query = query.Where(predicate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
            _dbSet.Attach(entity);
        entry.State = EntityState.Modified;
    }

    public void Delete(TEntity entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
            _dbSet.Attach(entity);
        _dbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.AnyAsync(cancellationToken)
            : await _dbSet.AnyAsync(predicate, cancellationToken);
    }


    public async Task<(IEnumerable<TResult> Items, int TotalCount)> QueryAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryBuilder = null,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeBuilder = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (includeBuilder != null)
            query = includeBuilder(query);

        if (queryBuilder != null)
            query = queryBuilder(query);

        int totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
            query = orderBy(query);

        if (skip.HasValue) query = query.Skip(skip.Value);
        if (take.HasValue) query = query.Take(take.Value);

        if (selector != null)
            return (await query.Select(selector).ToListAsync(cancellationToken), totalCount);

        var result = await query.ToListAsync(cancellationToken);
        return ((IEnumerable<TResult>)result, totalCount);
    }

    public async Task<TResult?> GetSingleAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>>? selector = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeBuilder = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (includeBuilder != null)
            query = includeBuilder(query);

        query = query.Where(predicate);

        if (selector != null)
            return await query.Select(selector).FirstOrDefaultAsync(cancellationToken);

        return await query.Cast<TResult>().FirstOrDefaultAsync(cancellationToken);
    }
}

