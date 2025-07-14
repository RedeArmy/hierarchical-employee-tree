namespace EmployeeHierarchy.Domain.Entities;

public class User
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public string Password { get; set; } = null!;

    public string? Role { get; set; } = "EMPLOYEE";

    public int? EmployeeId { get; set; }

    public DateTime CreateTime { get; set; }
}