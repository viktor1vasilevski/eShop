namespace eShop.Domain.Interfaces.EntityFramework;

public interface IEfUnitOfWork : IDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void SaveChanges();
}
