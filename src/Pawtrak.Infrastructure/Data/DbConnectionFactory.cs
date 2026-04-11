using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Pawtrak.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Pawtrak")
            ?? throw new InvalidOperationException("Connection string 'Pawtrak' is not configured.");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
