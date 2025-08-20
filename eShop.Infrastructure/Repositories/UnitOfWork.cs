using eShop.Domain.Interfaces;
using eShop.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace eShop.Infrastructure.Repositories
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public void DetachAllEntities()
        {
            var entities = _context.ChangeTracker.Entries();
            foreach (var entry in entities)
            {
                entry.State = EntityState.Detached;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IRepositoryBase<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return new RepositoryBase<TEntity>(_context);
        }

        public void RevertChanges()
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        {
                            entry.CurrentValues.SetValues(entry.OriginalValues);
                            entry.State = EntityState.Unchanged;
                            break;
                        }
                    case EntityState.Deleted:
                        {
                            entry.State = EntityState.Unchanged;
                            break;
                        }
                    case EntityState.Added:
                        {
                            entry.State = EntityState.Detached;
                            break;
                        }
                }
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
