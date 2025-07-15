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

    public async Task<IEnumerable<Position>> GetPositionsAsync()
    {
        const string sql = $"SELECT * FROM position ORDER BY position_id ASC";
        return await this.connection.QueryAsync<Position>(sql);
    }

    public async Task<IEnumerable<Employee>> GetManagersAsync()
    {
        const string sql =
            $@"SELECT employee_id, employee_first_name, employee_last_name FROM employee WHERE employee_id IN (SELECT DISTINCT manager_employee_id FROM employee WHERE manager_employee_id IS NOT NULL)";

        return await this.connection.QueryAsync<Employee>(sql);
    }
}