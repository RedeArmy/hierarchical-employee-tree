namespace EmployeeHierarchy.Domain.Models.Request;

public class DeleteEmployeeRequest
{
    public int EmployeeId { get; set; }

    public int? DeletedByUserId { get; set; }
}