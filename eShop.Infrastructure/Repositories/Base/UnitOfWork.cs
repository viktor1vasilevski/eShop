using eShop.Domain.Interfaces.Base;
using eShop.Infrastructure.Context;

namespace eShop.Infrastructure.Repositories.Base;

public class UnitOfWork(AppDbContext _context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);
    public void SaveChanges() => _context.SaveChanges();
    public void Dispose() => _context.Dispose();
}
