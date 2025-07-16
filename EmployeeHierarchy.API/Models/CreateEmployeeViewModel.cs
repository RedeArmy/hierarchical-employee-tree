namespace EmployeeHierarchy.API.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Domain.Entities;

public class CreateEmployeeViewModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Position")]
    public int PositionId { get; set; }

    [Display(Name = "Manager")]
    public int? ManagerEmployeeId { get; set; }

    public bool CreateUser { get; set; }

    public IEnumerable<SelectListItem> Positions { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Managers { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> PositionsManger { get; set; } = new List<SelectListItem>();
}