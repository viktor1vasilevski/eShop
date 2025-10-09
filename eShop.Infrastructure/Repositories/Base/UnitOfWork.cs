using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Infrastructure.Context;

namespace eShop.Infrastructure.Repositories.Base;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        if (_repositories.TryGetValue(typeof(TEntity), out var repo))
            return (IRepository<TEntity>)repo;

        var repository = new EfRepository<TEntity>(_context);
        _repositories[typeof(TEntity)] = repository;
        return repository;
    }

    public IEfRepository<TEntity> GetEfRepository<TEntity>() where TEntity : class
    {
        if (_repositories.TryGetValue(typeof(TEntity), out var repo))
            return (IEfRepository<TEntity>)repo;

        var repository = new EfRepository<TEntity>(_context);
        _repositories[typeof(TEntity)] = repository;
        return repository;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    public void SaveChanges() => _context.SaveChanges();
    public void Dispose() => _context.Dispose();
}
