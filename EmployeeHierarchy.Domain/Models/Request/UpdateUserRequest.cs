namespace EmployeeHierarchy.Domain.Models.Request;

public class UpdateUserRequest
{
    public int UserId { get; set; }

    public string? NewPassword { get; set; }

    public string? NewRole { get; set; }

    public bool? IsActive { get; set; }

    public int? UpdatedByUserId { get; set; }
}