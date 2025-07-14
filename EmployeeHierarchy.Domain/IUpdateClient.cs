namespace EmployeeHierarchy.Domain;

using Entities;
using System.Threading.Tasks;

public interface IUpdateClient
{
    Task<Employee> UpdateEmployeeInfoAsync(int employeeId, int? newPositionId, int? newManagerId, int updatedByUserId);

    Task<User> UpdateUserInfoAsync(int userId, string? newPassword, string? newRole, bool? isActive, int updatedByUserId);
}