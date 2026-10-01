using FixedApp.Authorization;
using FixedApp.Middlewares;
using FixedApp.Models;
using HashidsNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>
    (o => o.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IHashids>(new Hashids("BacReasearchSecureSalt", 11));
builder.Services.AddScoped<IAuthorizationHandler, IsOwnerHandler>();
builder.Services.AddAuthentication("BacCookieAuth").AddCookie("BacCookieAuth",
    o =>
    {
        o.Cookie.Name = "FixedApp.AuthCookie";
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseMiddleware<PerfLoggingMdw>();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();