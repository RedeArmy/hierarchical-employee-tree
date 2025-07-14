namespace EmployeeHierarchy.Domain;

using Entities;
using System.Threading.Tasks;

public interface IInsertClient
{
    Task<Employee> InsertEmployeeAsync(Employee employee, int createdByUserId);

    Task<Position> InsertPositionAsync(Position position, int createdByUserId);

    Task<User> InsertUserAsync(User user, int createdByUserId);
}