using System.Data;

namespace eShop.Domain.Interfaces.Dapper;

public interface IDapperUnitOfWork : IDisposable
{
    IDbTransaction BeginTransaction();
    void Commit();
    void Rollback();
}
