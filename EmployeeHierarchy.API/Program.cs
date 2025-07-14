using System.Data;
using EmployeeHierarchy.Domain;
using EmployeeHierarchy.Infrastructure.Database;
using EmployeeHierarchy.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar sesión
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ICreateClient, CreateClient>();
builder.Services.AddScoped<IUpdateClient, UpdateClient>();
builder.Services.AddScoped<IDeleteClient, DeleteClient>();
builder.Services.AddScoped<IReadClient, ReadClient>();
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

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
