using EmployeeHierarchy.Domain.Entities;

namespace EmployeeHierarchy.Infrastructure.Repositories;

using Domain.Models.Response;
using System.Data;
using Dapper;
using Domain;

public class ReadClient : IReadClient
{
    private readonly IDbConnection connection;

    public ReadClient(IDbConnection connection)
    {
        this.connection = connection;
    }

    public async Task<IEnumerable<EmployeeHierarchyResponse>> GetEmployeeHierarchyAsync()
    {
        var result = await this.connection.QueryAsync<EmployeeHierarchyResponse>(
            "sp_get_employee_hierarchy",
            commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var user = await this.connection.QueryFirstOrDefaultAsync<User>(
            $"SELECT * FROM [user] WHERE username = @username", new { username });

        if (user == null)
        {
            return null;
        }

        var verified = BCrypt.Net.BCrypt.Verify(password, user.Password);
        return verified ? user : null;
    }
}