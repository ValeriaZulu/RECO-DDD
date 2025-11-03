using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RECO.Infrastructure.TMDbClient;
using RECO.Infrastructure.Persistence;
using RECO.Infrastructure.Worker;
using Microsoft.EntityFrameworkCore;
using System;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // DbContext configuration - uses DATABASE_URL or default local
        var conn = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Host=localhost;Database=reco;Username=postgres;Password=postgres";
        services.AddDbContext<RECODbContext>(opt => opt.UseNpgsql(conn));

        // TMDb Http client - singleton via IHttpClientFactory as recommended in constitution
        services.AddHttpClient<ITMDbClient, TMDbAdapter>();

        services.AddHostedService<SyncWorker>();
    })
    .Build();

await host.RunAsync();
