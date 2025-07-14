using System.Text;

namespace EmployeeHierarchy.API.Controllers;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Models;
using Domain;
using System.Net.Http.Headers;
using System.Text.Json;
using EmployeeHierarchy.Domain.Models.ViewModels;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IReadClient readClient;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IReadClient readClient)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.readClient = readClient;
    }

    public IActionResult Index()
    {
        return this.RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        this.HttpContext.Session.Clear();
        return this.RedirectToAction("Login");
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

        var client = this.httpClientFactory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"http://localhost:5091/api/Employees/login", content);

        if (response.IsSuccessStatusCode)
        {
            this.HttpContext.Session.SetString("Username", model.Username);
            return this.RedirectToAction("Hierarchy");
        }

        this.ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return this.View(model);
    }

    public async Task<IActionResult> Hierarchy()
    {
        var client = this.httpClientFactory.CreateClient();

        var response = await client.GetAsync($"http://localhost:5091/api/Employees/get_hierarchy");
        if (!response.IsSuccessStatusCode)
        {
            return this.View("Error");
        }

        var json = await response.Content.ReadAsStringAsync();
        var hierarchy = JsonSerializer.Deserialize<List<EmployeeViewModel>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        return this.View(hierarchy);
    }

    public IActionResult Privacy() => this.View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}