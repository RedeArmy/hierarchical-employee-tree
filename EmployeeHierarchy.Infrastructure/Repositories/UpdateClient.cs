namespace EmployeeHierarchy.Infrastructure.Repositories;

using System.Data;
using Dapper;
using Domain;
using Utils;
using Domain.Entities;


public class UpdateClient : IUpdateClient
{
    private readonly IDbConnection connection;

    public UpdateClient(IDbConnection connection)
    {
        this.connection = connection;
    }

    public async Task<Employee> UpdateEmployeeInfoAsync(int employeeId, int? newPositionId, int? newManagerId, int updatedByUserId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@employee_id", employeeId);
        parameters.Add("@new_position_id", newPositionId);
        parameters.Add("@new_manager_id", newManagerId);
        parameters.Add("@updated_by_user_id", updatedByUserId);

        var updatedEmployee = await this.connection.QuerySingleOrDefaultAsync<Employee>(
            "sp_update_employee",
            parameters,
            commandType: CommandType.StoredProcedure);

        if (updatedEmployee == null)
        {
            throw new InvalidOperationException("Employee update failed or not found.");
        }

        return updatedEmployee;
    }

    public async Task<User> UpdateUserInfoAsync(int userId, string? newPassword, string? newRole, bool? isActive, int updatedByUserId)
    {
        string? defaultPassword = null;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            defaultPassword = HashPasswordHelper.HashPassword(newPassword);
        }

        var parameters = new DynamicParameters();
        parameters.Add("@user_id", userId);
        parameters.Add("@new_password", defaultPassword);
        parameters.Add("@new_rol", newRole);
        parameters.Add("@new_is_active", isActive);
        parameters.Add("@updated_by_user_id", updatedByUserId);

        var updatedUser = await this.connection.QuerySingleOrDefaultAsync<User>(
            "sp_update_user",
            parameters,
            commandType: CommandType.StoredProcedure);

        if (updatedUser == null)
        {
            throw new InvalidOperationException($"No user found or updated with ID {userId}.");
        }

        return updatedUser;
    }
}