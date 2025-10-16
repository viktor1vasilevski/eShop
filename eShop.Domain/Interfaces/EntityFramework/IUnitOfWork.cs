namespace eShop.Domain.Interfaces.EntityFramework;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    void SaveChanges();
}
