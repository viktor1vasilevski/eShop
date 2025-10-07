namespace eShop.Domain.Interfaces.Base;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    IEfRepository<TEntity> GetEfRepository<TEntity>() where TEntity : class;
    Task SaveChangesAsync();
    void SaveChanges();
}
