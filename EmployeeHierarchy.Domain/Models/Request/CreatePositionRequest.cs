namespace EmployeeHierarchy.Domain.Models.Request;

public class CreatePositionRequest
{
    public int PositionId { get; set; }

    public string PositionName { get; set; }

    public int? createdByUserId { get; set; }
}