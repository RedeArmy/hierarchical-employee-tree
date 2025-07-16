using EmployeeHierarchy.Domain.Entities;

namespace EmployeeHierarchy.API.Controllers;

using System.Text;
using Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Models;
using Domain;
using System.Text.Json;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IReadClient readClient;
    private readonly HttpClient client;
    private readonly string? baseUrl;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, IReadClient readClient)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.readClient = readClient;
        this.client = httpClientFactory.CreateClient("ApiClient");
        this.baseUrl = configuration["ApiSettings:BaseUrl"];
    }

    [SessionAuth]
    public IActionResult Index()
    {
        return this.RedirectToAction("Login");
    }

    [SessionAuth]
    public IActionResult Home()
    {
        return this.View("Index");
    }

    public IActionResult Logout()
    {
        this.HttpContext.Session.Clear();
        return this.RedirectToAction("Login");
    }

    [SessionAuth]
    public IActionResult Privacy() => this.View();

    [SessionAuth]
    [HttpGet]
    public async Task<IActionResult> Upload()
    {
        var positions = await this.readClient.GetPositionsAsync();
        var managers = await this.readClient.GetManagersAsync();

        var enumerable = positions as Position[] ?? positions.ToArray();

        var model = new CreateEmployeeViewModel
        {
            Positions = enumerable.Select(p => new SelectListItem
            {
                Value = p.Position_Id.ToString(),
                Text = p.Position_Name,
            }),
            Managers = managers.Select(m => new SelectListItem
            {
                Value = m.Employee_Id.ToString(),
                Text = $"{m.FirstName} {m.LastName}",
            }),
            PositionsManger = enumerable.Select(p => new SelectListItem
            {
                Value = p.Position_Id.ToString(),
                Text = p.Position_Name,
            }),
        };

        return this.View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return this.View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

        var response = await this.client.PostAsync($"{this.baseUrl}/api/Employees/login", content);

        if (response.IsSuccessStatusCode)
        {
            this.HttpContext.Session.SetString("Username", model.Username);
            return this.RedirectToAction("Home");
        }

        this.ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return this.View(model);
    }

    public async Task<IActionResult> Hierarchy()
    {
        var response = await this.client.GetAsync($"{this.baseUrl}/api/Employees/get_hierarchy");
        if (!response.IsSuccessStatusCode)
        {
            return this.View("Error");
        }

        var json = await response.Content.ReadAsStringAsync();
        var hierarchy = JsonSerializer.Deserialize<List<EmployeeViewModel>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        if (hierarchy == null || !hierarchy.Any())
        {
            return this.RedirectToAction("Upload");
        }


        return this.View(hierarchy);
    }

    [SessionAuth]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            this.ModelState.AddModelError(string.Empty, "Please select a CSV file.");
            return this.View();
        }

        using var reader = new StreamReader(csvFile.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        // Aquí puedes hacer parsing del CSV y enviar los datos a tu API para insertar en DB

        // Por simplicidad, redirigimos a la jerarquía después de subir
        return this.RedirectToAction("Hierarchy");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}