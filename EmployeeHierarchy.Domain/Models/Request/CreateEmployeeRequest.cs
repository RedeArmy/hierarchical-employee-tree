namespace EmployeeHierarchy.Domain.Models.Request;

public class CreateEmployeeRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public int PositionId { get; set; }

    public int? ManagerEmployeeId { get; set; }

    public int? createdByUserId { get; set; }

    public bool CreateUser { get; set; } = false;
}