using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RECO.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RECO.Application.Clients;
using RECO.Application.Interfaces;

namespace RECO.Infrastructure.Worker
{
    // Background worker that performs a limited sync for demo/testing
    public class SyncWorker : BackgroundService
    {
        private readonly ILogger<SyncWorker> _logger;
        private readonly IImportService _importService;
        private readonly ITMDbClient _tmdb;
        private readonly RECODbContext _db;

        public SyncWorker(ILogger<SyncWorker> logger, IImportService importService, ITMDbClient tmdb, RECODbContext db)
        {
            _logger = logger;
            _importService = importService;
            _tmdb = tmdb;
            _db = db;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SyncWorker starting (demo limited run)...");

            // For demo we run once and exit
            try
            {
                _logger.LogInformation("Fetching trending titles from TMDb...");
                var trending = await _tmdb.GetTrendingAsync();
                foreach (var item in trending)
                {
                    try
                    {
                        _logger.LogInformation("Importing {Title} ({Id})...", item.Title, item.Id);
                        Console.WriteLine($"Importing {item.Title} ({item.Id})...");
                        await _importService.ImportFromTMDbAsync(item.Id, item.MediaType);
                        _logger.LogInformation("Imported {Id}", item.Id);
                    }
                    catch (Exception inner)
                    {
                        _logger.LogError(inner, "Failed to import {Id}", item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during TMDb sync");
            }

            _logger.LogInformation("SyncWorker demo run completed. Stopping host.");
            // When run as a demo, stop the host
            await Task.Delay(1000, stoppingToken);
            Environment.Exit(0);
        }
    }
}
