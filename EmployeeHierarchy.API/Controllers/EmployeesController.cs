namespace EmployeeHierarchy.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Domain.Models.Request;
using Domain;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController
    (IEmployeeClient employeeClient)
    : ControllerBase
{
    [HttpPost("create-employee")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PositionId = request.PositionId,
            ManagerEmployeeId = request.ManagerEmployeeId,
            CreateUser = request.CreateUser,
        };

        var createdByUserId = request.createdByUserId ?? 2;
        var result = await employeeClient.InsertEmployeeAsync(employee, createdByUserId);

        return this.Ok(result);
    }

    [HttpPost("create-position")]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var position = new Position
        {
            PositionName = request.PositionName,
        };

        var createdByUserId = request.createdByUserId ?? 2;
        var result = await employeeClient.InsertPositionAsync(position, createdByUserId);

        return this.Ok(result);
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var user = new User
        {
            Username = request.Username,
            Role = request.Role,
            EmployeeId = request.EmployeeId,
        };

        var createdByUserId = request.createdByUserId ?? 2;
        var result = await employeeClient.InsertUserAsync(user, createdByUserId);

        return this.Ok(result);
    }
}