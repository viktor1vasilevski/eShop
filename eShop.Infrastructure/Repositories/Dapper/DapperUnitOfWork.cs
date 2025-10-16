using eShop.Domain.Interfaces.Dapper;
using System.Data;

namespace eShop.Infrastructure.Repositories.Dapper;

public class DapperUnitOfWork : IDapperUnitOfWork
{
    private readonly IDbConnection _dbConnection;
    private IDbTransaction? _transaction;

    public DapperUnitOfWork(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public IDbTransaction BeginTransaction()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        _transaction = _dbConnection.BeginTransaction();
        return _transaction;
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _dbConnection.Dispose();
    }
}
