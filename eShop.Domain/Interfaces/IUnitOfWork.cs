namespace eShop.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IRepositoryBase<TEntity> GetRepository<TEntity>() where TEntity : class;
        void Dispose();
        void SaveChanges();
        Task SaveChangesAsync();
        void RevertChanges();
        void DetachAllEntities();
    }
}
