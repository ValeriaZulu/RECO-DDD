using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using RECO.Infrastructure.Persistence;
using RECO.Infrastructure.TMDbClient;

var builder = WebApplication.CreateBuilder(args);

// DB - look for DATABASE_URL env var
var conn = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Database=reco;Username=postgres;Password=postgres";
builder.Services.AddDbContext<RECODbContext>(o => o.UseNpgsql(conn));

// TMDb client wiring - HttpClient via DI
builder.Services.AddHttpClient<ITMDbClient, TMDbAdapter>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
