using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RECO.Infrastructure.TMDbClient
{
    // Adapter implements the TMDb client and maps DTOs to domain. Adheres to Adapter pattern.
    public class TMDbAdapter : ITMDbClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public TMDbAdapter(HttpClient http, IConfiguration config)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _apiKey = config["TMDB_API_KEY"] ?? string.Empty;
            // Constitution: secrets MUST come from env vars / secret manager
        }

        public async Task<IEnumerable<TMDbDto>> GetPopularMoviesAsync(int page = 1)
        {
            if (string.IsNullOrEmpty(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");
            var url = $"https://api.themoviedb.org/3/movie/popular?api_key={_apiKey}&page={page}";
            var resp = await _http.GetFromJsonAsync<TMDbPopularResponse>(url);
            return resp?.Results ?? Array.Empty<TMDbDto>();
        }

        public async Task<TMDbDto?> GetDetailsAsync(int tmdbId, string type = "movie")
        {
            if (string.IsNullOrEmpty(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");
            var url = $"https://api.themoviedb.org/3/{type}/{tmdbId}?api_key={_apiKey}";
            var resp = await _http.GetFromJsonAsync<TMDbDetailsResponse>(url);
            return resp is null ? null : new TMDbDto(resp.Id, resp.Title ?? resp.Name ?? "", resp.Overview, resp.PosterPath);
        }
    }

    internal class TMDbPopularResponse
    {
        public int Page { get; set; }
        public TMDbDto[] Results { get; set; } = Array.Empty<TMDbDto>();
    }

    internal class TMDbDetailsResponse
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
    }
}
