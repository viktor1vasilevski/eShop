using eShop.Domain.Interfaces;
using eShop.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace eShop.Infrastructure.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(object id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        IQueryable<TEntity> query = _dbSet;
        if (predicate != null)
            query = query.Where(predicate);

        return await query.ToListAsync();
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
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

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null ? await _dbSet.AnyAsync() : await _dbSet.AnyAsync(predicate);
    }
}

