using EmployeeHierarchy.Domain.Entities;

namespace EmployeeHierarchy.Domain;

using Models.Response;
using System.Threading.Tasks;

public interface IReadClient
{
    Task<IEnumerable<EmployeeHierarchyResponse>> GetEmployeeHierarchyAsync();

    Task<User> ValidateUserAsync(string username, string password);
}