using System.Data;
using Dapper;
using Pawtrak.Core.Common;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbConnectionFactory _connectionFactory;
    protected abstract string TableName { get; }

    protected RepositoryBase(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(
            $"SELECT * FROM {TableName} WHERE Id = @Id",
            new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<T>($"SELECT * FROM {TableName}");
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        await InsertAsync(conn, entity);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        using var conn = _connectionFactory.CreateConnection();
        await UpdateCoreAsync(conn, entity);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync($"DELETE FROM {TableName} WHERE Id = @Id", new { entity.Id });
    }

    protected abstract Task InsertAsync(IDbConnection conn, T entity);
    protected abstract Task UpdateCoreAsync(IDbConnection conn, T entity);
}
