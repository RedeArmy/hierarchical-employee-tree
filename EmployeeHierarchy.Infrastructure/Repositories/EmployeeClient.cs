namespace EmployeeHierarchy.Infrastructure.Repositories;

using System.Data;
using BCrypt.Net;
using Dapper;
using Domain;
using Domain.Entities;


public class EmployeeClient : IEmployeeClient
{
    private readonly IDbConnection connection;

    public EmployeeClient(IDbConnection connection)
    {
        this.connection = connection;
    }

    private static string HashPassword(string password)
    {
        return BCrypt.HashPassword(password);
    }

    public async Task<Employee> InsertEmployeeAsync(Employee employee, int createdByUserId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@first_name", employee.FirstName);
        parameters.Add("@last_name", employee.LastName);
        parameters.Add("@position_id", employee.PositionId);
        parameters.Add("@manager_id", employee.ManagerEmployeeId);
        parameters.Add("@created_by_user_id", createdByUserId);

        if (employee.CreateUser)
        {
            var username = await this.GenerateUniqueUsernameAsync(employee.FirstName, employee.LastName);
            var defaultPassword = string.Concat(username, "123");

            parameters.Add("@create_user", 1);
            parameters.Add("@username", username);
            parameters.Add("@password", HashPassword(defaultPassword));
        }

        var insertedEmployee = await this.connection.QuerySingleOrDefaultAsync<Employee>(
            "sp_insert_employee",
            parameters,
            commandType: CommandType.StoredProcedure);

        if (insertedEmployee == null)
        {
            throw new InvalidOperationException("No employee returned from insert stored procedure.");
        }

        return insertedEmployee;
    }

    public async Task<Position> InsertPositionAsync(Position position, int createdByUserId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@position_name", position.PositionName.ToUpper());
        parameters.Add("@created_by_user_id", createdByUserId);

        var insertedPosition = await this.connection.QuerySingleOrDefaultAsync<Position>(
            "sp_insert_position",
            parameters,
            commandType: CommandType.StoredProcedure);

        if (insertedPosition == null)
        {
            throw new InvalidOperationException("No position returned from insert stored procedure.");
        }

        return insertedPosition;
    }

    public async Task<User> InsertUserAsync(User user, int createdByUserId)
    {
        string userName;
        string defaultPassword;

        if (user.EmployeeId.HasValue)
        {
            var employee = await this.connection.QuerySingleOrDefaultAsync<(string FirstName, string LastName)>(
                "SELECT employee_first_name AS FirstName, employee_last_name AS LastName FROM employee WHERE employee_id = @EmployeeId",
                new { EmployeeId = user.EmployeeId });

            if (employee == default)
            {
                throw new InvalidOperationException($"No employee found with ID {user.EmployeeId}");
            }

            userName = await this.GenerateUniqueUsernameAsync(employee.FirstName, employee.LastName);
            defaultPassword = string.Concat(userName, "123");
        }
        else
        {
            userName = user.Username;
            defaultPassword = user.Username.ToLower();
        }

        var parameters = new DynamicParameters();
        parameters.Add("@username", userName);
        parameters.Add("@password", HashPassword(defaultPassword));
        parameters.Add("@role", user.Role ?? "EMPLOYEE");
        parameters.Add("@employee_id", user.EmployeeId, DbType.Int32);
        parameters.Add("@created_by_user_id", createdByUserId);

        var insertedUser = await this.connection.QuerySingleOrDefaultAsync<User>(
            "sp_insert_user",
            parameters,
            commandType: CommandType.StoredProcedure);

        if (insertedUser == null)
        {
            throw new InvalidOperationException("No user returned from insert stored procedure.");
        }

        return insertedUser;
    }

    private async Task<string> GenerateUniqueUsernameAsync(string firstName, string lastName)
    {
        var lastNameParts = lastName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lastNameFirstPart = lastNameParts.Length > 0 ? lastNameParts[0] : lastName;

        var baseUsername = (firstName[0] + lastNameFirstPart).ToLower();
        var username = baseUsername;
        var firstNameLetters = 1;
        var secondNameLetters = 0;

        while (await this.connection.QuerySingleAsync<int>("SELECT COUNT(1) FROM [user] WHERE username = @Username", new { Username = username }) > 0)
        {
            var names = firstName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (names.Length == 1)
            {
                firstNameLetters++;
                username = string.Concat(names[0].AsSpan(0, firstNameLetters), lastNameFirstPart).ToLower();
            }
            else
            {
                if (secondNameLetters < names[1].Length)
                {
                    secondNameLetters++;
                }
                else if (firstNameLetters < names[0].Length)
                {
                    firstNameLetters++;
                }

                username = string.Concat(names[0].AsSpan(0, firstNameLetters), names[1].AsSpan(0, secondNameLetters), lastNameFirstPart).ToLower();
            }
        }

        return username;
    }
}