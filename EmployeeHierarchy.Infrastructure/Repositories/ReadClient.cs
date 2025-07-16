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
            $@"SELECT employee_id AS EmployeeId, employee_first_name AS FirstName, employee_last_name AS LastName FROM employee WHERE employee_id IN (SELECT DISTINCT manager_employee_id FROM employee WHERE manager_employee_id IS NOT NULL)";

        return await this.connection.QueryAsync<Employee>(sql);
    }
    
    public async Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(int positionId)
    {
        return await this.connection.QueryAsync<Employee>(
            "SELECT employee_id AS EmployeeId, employee_first_name AS FirstName, employee_last_name AS LastName FROM employee WHERE position_id = @positionId", new { positionId });
    }
    
    public async Task<bool> EmployeeExistsAsync(string firstName, string lastName)
    {
        const string sql = @"
        SELECT COUNT(*) 
        FROM employee 
        WHERE employee_first_name = @firstName AND employee_last_name = @lastName";

        var count = await this.connection.ExecuteScalarAsync<int>(sql, new { firstName, lastName });
        return count > 0;
    }
}