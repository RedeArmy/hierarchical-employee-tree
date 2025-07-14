namespace EmployeeHierarchy.Domain;

using System.Threading.Tasks;

public interface IDeleteClient
{
    Task<bool> DeleteEmployeeAndReassignAsync(int employeeId, int deletedByUserId);
}