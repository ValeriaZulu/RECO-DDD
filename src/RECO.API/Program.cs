using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using RECO.Infrastructure.Persistence;
using RECO.Infrastructure.TMDbClient;
using MediatR;
using RECO.Application.DTOs;
using RECO.Infrastructure.Services; // 👈 IMPORTANTE

var builder = WebApplication.CreateBuilder(args);

// DB - look for DATABASE_URL env var
var conn = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Database=reco;Username=postgres;Password=postgres";
builder.Services.AddDbContext<RECODbContext>(o => o.UseNpgsql(conn));

// TMDb client wiring - HttpClient via DI
builder.Services.AddHttpClient<RECO.Application.Clients.ITMDbClient, RECO.Infrastructure.TMDbClient.TMDbAdapter>();

builder.Services.AddControllersWithViews();

// Register MediatR handlers from Application assembly. Uses DI by interface per constitution (DIP).
builder.Services.AddMediatR(typeof(ReviewDto).Assembly);

// ✅ Add Infrastructure services (repositories, etc.)
builder.Services.AddInfrastructureServices();

builder.Services.AddScoped<RECO.Domain.Interfaces.IDomainEventDispatcher, RECO.Infrastructure.Events.DomainEventDispatcher>();

var app = builder.Build();

// Register custom middleware: Error handling and request logging.
app.UseMiddleware<RECO.API.Middleware.ErrorHandlingMiddleware>();
app.UseMiddleware<RECO.API.Middleware.RequestLoggingMiddleware>();

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
