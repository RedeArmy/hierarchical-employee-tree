namespace EmployeeHierarchy.Domain.Models.Request;

public class LoginRequest
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
}