namespace EmployeeHierarchy.Infrastructure.Database;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DbConnectionFactory
{
    private readonly IConfiguration configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = this.configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Connection string: {connectionString}");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        var connection = new SqlConnection(connectionString);
        return connection;
    }
}