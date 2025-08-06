using eShop.Domain.Interfaces;
using eShop.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace eShop.Infrastructure.Repositories;

public class RepositoryBasey<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    internal AppDbContext _dbContext;
    internal DbSet<TEntity> dbSet;
    public RepositoryBasey(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        dbSet = _dbContext.Set<TEntity>();
    }


    public TEntity Delete(object id)
    {
        throw new NotImplementedException();
    }

    public void Delete(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void DeleteRange(Expression<Func<TEntity, bool>>? filter = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public bool Exists(Expression<Func<TEntity, bool>>? filter = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetAsQueryable(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetAsQueryableWhereIf(Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        throw new NotImplementedException();
    }

    public TEntity GetById(object id)
    {
        throw new NotImplementedException();
    }

    public TEntity Insert(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> InsertAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void InsertRange(IEnumerable<TEntity> entities)
    {
        throw new NotImplementedException();
    }

    public object SetObjectStateToAdded(object obj)
    {
        throw new NotImplementedException();
    }

    public object SetObjectStateToDetached(object obj)
    {
        throw new NotImplementedException();
    }

    public TEntity Update(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> UpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void UpdateWithRelatedEntities(TEntity entity)
    {
        throw new NotImplementedException();
    }
}
