namespace EmployeeHierarchy.Domain.Models.Request;

public class UpdateEmployeeRequest
{
    public int EmployeeId { get; set; }

    public int? NewPositionId { get; set; }

    public int? NewManagerId { get; set; }

    public int? UpdatedByUserId { get; set; }
}