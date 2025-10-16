using Dapper;
using eShop.Domain.Interfaces.Dapper;
using System.Data;

namespace eShop.Infrastructure.Repositories.Dapper;

public class DapperRepository<T>(IDbConnection _dbConnection) : IDapperRepository<T> where T : class
{
    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return await _dbConnection.QueryAsync<TResult>(command);
    }

    public async Task<TResult?> QuerySingleAsync<TResult>(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return await _dbConnection.QuerySingleOrDefaultAsync<TResult>(command);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return await _dbConnection.ExecuteAsync(command);
    }

    public async Task<object?> InsertAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return await _dbConnection.ExecuteScalarAsync<object>(command);
    }

    public async Task<int> UpdateAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return await _dbConnection.ExecuteAsync(command);
    }

    public async Task<int> DeleteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken);
        return await _dbConnection.ExecuteAsync(command);
    }
}
