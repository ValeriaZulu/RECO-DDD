using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RECO.Infrastructure.TMDbClient;
using RECO.Infrastructure.Persistence;
using RECO.Infrastructure.Worker;
using Microsoft.EntityFrameworkCore;
using System;
using DotNetEnv;
using RECO.Application.Services;
using RECO.Infrastructure.Services;

// Load environment variables from a local .env file (if present) so the worker can read TMDB_API_KEY and TMDB_ID during development.
// This is safe in dev/demo scenarios; production should provide environment variables via the hosting environment.

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        Env.Load();

        // DbContext configuration - uses DATABASE_URL or default local
        var conn = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Database=reco;Username=postgres;Password=postgres";
        services.AddDbContext<RECODbContext>(opt => opt.UseNpgsql(conn));

        // Register Infrastructure services (repositories, adapters)
        services.AddInfrastructureServices();

    // TMDb Http client - singleton via IHttpClientFactory as recommended in constitution
    services.AddHttpClient<RECO.Application.Clients.ITMDbClient, RECO.Infrastructure.TMDbClient.TMDbAdapter>();

    // Application services
    services.AddScoped<RECO.Application.Interfaces.IImportService, RECO.Application.Services.ImportService>();

        // Hosted worker
        services.AddHostedService<SyncWorker>();
    })
    .Build();

await host.RunAsync();
