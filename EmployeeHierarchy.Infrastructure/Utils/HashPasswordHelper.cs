namespace EmployeeHierarchy.Infrastructure.Utils;

using BCrypt.Net;

public static class HashPasswordHelper
{
    public static string HashPassword(string password)
    {
        return BCrypt.HashPassword(password);
    }
}