using System.Data;
using EmployeeHierarchy.Domain;
using EmployeeHierarchy.Infrastructure.Database;
using EmployeeHierarchy.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar configuración de Dapper y conexión a SQL
builder.Services.AddScoped<IInsertClient, InsertClient>();
builder.Services.AddScoped<IUpdateClient, UpdateClient>();
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IDbConnection>(sp =>
    sp.GetRequiredService<DbConnectionFactory>().CreateConnection());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
