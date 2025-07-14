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
    [HttpPost]
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
}