namespace EmployeeHierarchy.Domain.Models.Response;

public class EmployeeHierarchyResponse
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PositionName { get; set; }

    public int? ManagerId { get; set; }

    public int? HierarchyLevel { get; set; }
}