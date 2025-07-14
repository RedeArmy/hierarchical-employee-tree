namespace EmployeeHierarchy.Domain;

using Models.Response;
using System.Threading.Tasks;

public interface IReadClient
{
    Task<IEnumerable<EmployeeHierarchyResponse>> GetEmployeeHierarchyAsync();
}