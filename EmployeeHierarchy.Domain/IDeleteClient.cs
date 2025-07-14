namespace EmployeeHierarchy.Domain;

using Entities;
using System.Threading.Tasks;

public interface IDeleteClient
{
    Task<bool> DeleteEmployeeAndReassignAsync(int employeeId, int deletedByUserId);
}