using eShop.Domain.Interfaces;
using eShop.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace eShop.Infrastructure.Repositories;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    internal AppDbContext _dbContext;
    internal DbSet<TEntity> dbSet;
    public RepositoryBase(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        dbSet = _dbContext.Set<TEntity>();
    }


    public TEntity Delete(object id)
    {
        TEntity entity = dbSet.Find(id);
        Delete(entity);
        return entity;
    }

    public void Delete(TEntity entity)
    {
        if (_dbContext.Entry(entity).State == EntityState.Detached)
        {
            dbSet.Attach(entity);
        }
        dbSet.Remove(entity);
    }

    public void DeleteRange(Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
            dbSet.RemoveRange(query);
        }
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
        }
        dbSet.RemoveRange(entities);
    }

    public bool Exists(Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = dbSet;
        return query.Any(filter);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        IQueryable<TEntity> query = dbSet;
        return await query.AnyAsync(filter ?? (_ => true));
    }

    public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        IQueryable<TEntity> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        if (include != null)
        {
            query = include(query);
        }
        return query.ToList();
    }

    public IQueryable<TEntity> GetAsQueryable(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        IQueryable<TEntity> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        if (include != null)
        {
            query = include(query);
        }
        return query;
    }

    public IQueryable<TEntity> GetAsQueryableWhereIf(Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        IQueryable<TEntity> query = dbSet;

        if (filter != null)
        {
            query = filter(query);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (include != null)
        {
            query = include(query);
        }

        return query;
    }

    public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        IQueryable<TEntity> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        if (include != null)
        {
            query = include(query);
        }
        return await query.ToListAsync();
    }

    public TEntity GetById(object id)
    {
        IQueryable<TEntity> query = dbSet;
        return dbSet.Find(id);
    }

    public TEntity Insert(TEntity entity)
    {
        dbSet.Add(entity);
        return entity;
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        await dbSet.AddAsync(entity);
        return entity;
    }

    public void InsertRange(IEnumerable<TEntity> entities)
    {
        dbSet.AddRange(entities);
    }

    public object SetObjectStateToAdded(object obj)
    {
        _dbContext.Entry(obj).State = EntityState.Added;
        return obj;
    }

    public object SetObjectStateToDetached(object obj)
    {
        _dbContext.Entry(obj).State = EntityState.Detached;
        return obj;
    }

    public TEntity Update(TEntity entity)
    {
        var entry = _dbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<TEntity>().Attach(entity);
        }
        entry.State = EntityState.Modified;
        return entity;
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        var updated = Update(entity);
        return Task.FromResult(updated);
    }

    public TEntity UpdateWithRelatedEntities(TEntity entity)
    {
        var entry = _dbContext.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<TEntity>().Attach(entity);
        }

        entry.State = EntityState.Modified;

        foreach (var reference in entry.References)
        {
            if (reference.TargetEntry != null && reference.TargetEntry.Metadata.IsOwned())
            {
                reference.TargetEntry.State = EntityState.Modified;
            }
        }

        return entity;
    }


}
