using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RECO.Infrastructure.TMDbClient;
using RECO.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RECO.Infrastructure.Worker
{
    // Background worker that performs a limited sync for demo/testing
    public class SyncWorker : BackgroundService
    {
        private readonly ILogger<SyncWorker> _logger;
        private readonly ITMDbClient _tmdb;
        private readonly RECODbContext _db;

        public SyncWorker(ILogger<SyncWorker> logger, ITMDbClient tmdb, RECODbContext db)
        {
            _logger = logger;
            _tmdb = tmdb;
            _db = db;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SyncWorker starting (demo limited run)...");

            // For demo we run once and exit
            try
            {
                var items = await _tmdb.GetPopularMoviesAsync(1);
                foreach (var it in items)
                {
                    _logger.LogInformation("Found TMDb item {Id} - {Title}", it.Id, it.Title);
                    // Minimal upsert: ensure Titles table exists and add a row - simplified for demo
                    // Note: real mapping to domain entities and upsert via repository should be used.
                    // This method keeps worker as a small integration test scaffold per M3.
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
