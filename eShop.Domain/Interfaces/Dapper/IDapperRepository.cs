using System.Data;

namespace eShop.Domain.Interfaces.Dapper;

public interface IDapperRepository<T> where T : class
{
    Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<TResult?> QuerySingleAsync<TResult>(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<object?> InsertAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
}
