namespace EmployeeHierarchy.Domain.Models.Request;

public class CreateUserRequest
{
    public string? Username { get; set; }

    public string? Role { get; set; }

    public int? EmployeeId { get; set; }

    public int? createdByUserId { get; set; }

    public DateTime CreateTime { get; set; }
}