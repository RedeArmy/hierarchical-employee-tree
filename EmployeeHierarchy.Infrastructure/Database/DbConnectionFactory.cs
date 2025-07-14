namespace EmployeeHierarchy.Infrastructure.Database;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DbConnectionFactory
{
    private readonly IConfiguration configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        return new SqlConnection(connectionString);
    }
}