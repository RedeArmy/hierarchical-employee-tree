using EmployeeHierarchy.Domain.Entities;

namespace EmployeeHierarchy.Domain;

using Models.Response;
using System.Threading.Tasks;

public interface IReadClient
{
    Task<IEnumerable<EmployeeHierarchyResponse>> GetEmployeeHierarchyAsync();

    Task<User> ValidateUserAsync(string username, string password);

    Task<IEnumerable<Position>> GetPositionsAsync();

    Task<IEnumerable<Employee>> GetManagersAsync();

    Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(int positionId);

    Task<bool> EmployeeExistsAsync(string firstName, string lastName);
}