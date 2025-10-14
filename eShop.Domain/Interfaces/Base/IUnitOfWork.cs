namespace eShop.Domain.Interfaces.Base;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void SaveChanges();
}
