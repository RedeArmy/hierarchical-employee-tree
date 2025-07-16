namespace EmployeeHierarchy.API.Models;

public class EmployeeViewModel
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PositionName { get; set; } = string.Empty;

    public int? ManagerId { get; set; }

    public int? HierarchyLevel { get; set; }

    public List<EmployeeViewModel> Subordinates { get; set; } = new List<EmployeeViewModel>();

    public string FullName => $"{this.FirstName} {this.LastName}";
}