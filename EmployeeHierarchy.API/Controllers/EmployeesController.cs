namespace EmployeeHierarchy.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Domain.Models.Request;
using Domain;
using Microsoft.Data.SqlClient;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController
    (ICreateClient createClient, IUpdateClient updateClient, IDeleteClient deleteClient, IReadClient readClient)
    : ControllerBase
{
    [HttpPost("create-employee")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var exists = await readClient.EmployeeExistsAsync(request.FirstName, request.LastName);
        if (exists)
        {
            return this.Conflict(new { message = "An employee with this full name already exists." });
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
        var result = await createClient.InsertEmployeeAsync(employee, createdByUserId);

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
        var result = await createClient.InsertPositionAsync(position, createdByUserId);

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
        var result = await createClient.InsertUserAsync(user, createdByUserId);

        return this.Ok(result);
    }

    [HttpPost("update-employee")]
    public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var updatedByUserId = request.UpdatedByUserId ?? 2;
        var updatedEmployee = await updateClient.UpdateEmployeeInfoAsync(
            request.EmployeeId,
            request.NewPositionId,
            request.NewManagerId,
            updatedByUserId);

        return this.Ok(updatedEmployee);
    }

    [HttpPost("update-user")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var updatedByUserId = request.UpdatedByUserId ?? 2;

        var updatedUser = await updateClient.UpdateUserInfoAsync(
            request.UserId,
            request.NewPassword,
            request.NewRole,
            request.IsActive,
            updatedByUserId);

        return this.Ok(updatedUser);
    }

    [HttpDelete("delete-employee")]
    public async Task<IActionResult> DeleteEmployee([FromBody] DeleteEmployeeRequest request)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var updatedByUserId = request.DeletedByUserId ?? 2;

            var result = await deleteClient.DeleteEmployeeAndReassignAsync(
                request.EmployeeId,
                updatedByUserId);

            return this.Ok(new { success = result });
        }
        catch (SqlException ex)
        {
            return this.StatusCode(500, new { error = "Database error", details = ex.Message });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    [HttpGet("get_hierarchy")]
    public async Task<IActionResult> GetHierarchyTree()
    {
        var hierarchy = await readClient.GetEmployeeHierarchyAsync();
        return this.Ok(hierarchy);
    }

    [HttpGet("get_managers_position")]
    public async Task<IActionResult> GetManagersByPosition(int positionId)
    {
        var managers = await readClient.GetEmployeesByPositionAsync(positionId);
        var result = managers.Select(m => new
        {
            employeeId = m.EmployeeId,
            firstName = m.FirstName,
            lastName = m.LastName,
        });

        return new JsonResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await readClient.ValidateUserAsync(request.Username, request.Password);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (user == null)
        {
            return this.Unauthorized(new { message = "Invalid username or password" });
        }

        return this.Ok();
    }
}