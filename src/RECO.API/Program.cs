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

Environment.SetEnvironmentVariable("JWT_SECRET", "RECO_SUPER_SECURE_KEY_2025_987654321!");
var builder = WebApplication.CreateBuilder(args);

// DB - look for DATABASE_URL env var
var conn = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Database=reco;Username=postgres;Password=postgres";
builder.Services.AddDbContext<RECODbContext>(o => o.UseNpgsql(conn));

// TMDb client wiring - HttpClient via DI
builder.Services.AddHttpClient<RECO.Application.Clients.ITMDbClient, RECO.Infrastructure.TMDbClient.TMDbAdapter>();

// JWT and authentication
builder.Services.AddSingleton<RECO.API.Services.JwtService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? "RECO_DEV_SECRET_ChangeMe_To_A_Much_Longer_Key_123456789";

        var key = System.Text.Encoding.UTF8.GetBytes(secret);
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
        };

        // Also allow JWT to be read from cookie named "jwt" for browser flows
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token))
                {
                    if (ctx.Request.Cookies.TryGetValue("jwt", out var cookieToken)) ctx.Token = cookieToken;
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

// Register MediatR handlers from Application assembly. Uses DI by interface per constitution (DIP).
builder.Services.AddMediatR(typeof(ReviewDto).Assembly);

// ✅ Add Infrastructure services (repositories, etc.)
builder.Services.AddInfrastructureServices();

builder.Services.AddScoped<RECO.Domain.Interfaces.IDomainEventDispatcher, RECO.Infrastructure.Events.DomainEventDispatcher>();

var app = builder.Build();

// ✅ Middlewares de errores y logging
app.UseMiddleware<RECO.API.Middleware.ErrorHandlingMiddleware>();
app.UseMiddleware<RECO.API.Middleware.RequestLoggingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

// Routing must come before authentication so endpoint data is available
app.UseRouting();

// Populate ctx.User from tokens/cookies
app.UseAuthentication();

// Custom redirect middleware: if the user is NOT authenticated and requests the root/home
// redirect to /login. This runs before UseAuthorization so the authorization middleware
// won't short-circuit the request with a 401 before we can redirect.
app.Use(async (ctx, next) =>
{
    if (ctx.User?.Identity != null && ctx.User.Identity.IsAuthenticated)
    {
        await next();
        return;
    }

    var path = ctx.Request.Path.Value ?? string.Empty;

    // Allow anonymous access to public paths
    if (path.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/register", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/Auth/", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/Views/", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    if (string.IsNullOrEmpty(path) || path == "/" || path.Equals("/home", StringComparison.OrdinalIgnoreCase) || path.Equals("/home/index", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.Redirect("/login");
        return;
    }

    await next();
});

// Authorization should run after our redirect middleware so it doesn't return 401 for unauthenticated
// requests to root before we have a chance to redirect them.
app.UseAuthorization();

// Conventional MVC route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
