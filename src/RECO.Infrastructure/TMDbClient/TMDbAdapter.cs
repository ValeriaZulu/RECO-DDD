using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RECO.Application.Clients;
using RECO.Application.DTOs;

namespace RECO.Infrastructure.TMDbClient
{
    /// <summary>
    /// Simple TMDb adapter that implements ITMDbClient. Reads API key from environment variables.
    /// Infrastructure contains HTTP logic; Application only depends on the abstraction (DIP).
    /// </summary>
    public class TMDbAdapter : RECO.Application.Clients.ITMDbClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TMDbAdapter> _logger;
        private readonly string? _apiKey;

        public TMDbAdapter(HttpClient http, ILogger<TMDbAdapter> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = Environment.GetEnvironmentVariable("TMDB_API_KEY");
            if (string.IsNullOrWhiteSpace(_apiKey)) _logger.LogWarning("TMDB_API_KEY not set in environment");
        }

        public async Task<MovieDto> GetMovieDetailsAsync(int tmdbId)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");

            var url = $"https://api.themoviedb.org/3/movie/{tmdbId}?api_key={_apiKey}&language=en-US";
            var res = await _http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            using var s = await res.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            var root = doc.RootElement;

            var dto = new MovieDto
            {
                Id = root.GetProperty("id").GetInt32(),
                Title = root.GetProperty("title").GetString() ?? string.Empty,
                Overview = root.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                PosterPath = root.TryGetProperty("poster_path", out var pp) && pp.GetString() is string p ? $"https://image.tmdb.org/t/p/w500{p}" : null,
            };

            if (root.TryGetProperty("release_date", out var rd) && DateTime.TryParse(rd.GetString(), out var d)) dto.ReleaseDate = d;
            if (root.TryGetProperty("genres", out var genres) && genres.ValueKind == JsonValueKind.Array)
            {
                foreach (var g in genres.EnumerateArray())
                {
                    if (g.TryGetProperty("name", out var name)) dto.Genres.Add(name.GetString() ?? string.Empty);
                }
            }

            // explicit media type for movie details
            dto.MediaType = "movie";

            return dto;
        }

        public async Task<MovieDto> GetDetailsAsync(int tmdbId, string? mediaType = null)
        {
            // Decide endpoint based on mediaType or fallback to movie
            if (string.Equals(mediaType, "tv", StringComparison.OrdinalIgnoreCase))
            {
                // call tv details
                if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");
                var url = $"https://api.themoviedb.org/3/tv/{tmdbId}?api_key={_apiKey}&language=en-US";
                var res = await _http.GetAsync(url);
                res.EnsureSuccessStatusCode();
                using var s = await res.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(s);
                var root = doc.RootElement;

                var dto = new MovieDto
                {
                    Id = root.GetProperty("id").GetInt32(),
                    Title = root.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty,
                    Overview = root.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                    PosterPath = root.TryGetProperty("poster_path", out var pp) && pp.GetString() is string p ? $"https://image.tmdb.org/t/p/w500{p}" : null,
                    MediaType = "tv"
                };
                if (root.TryGetProperty("first_air_date", out var fa) && DateTime.TryParse(fa.GetString(), out var fd)) dto.ReleaseDate = fd;
                if (root.TryGetProperty("genres", out var genres) && genres.ValueKind == JsonValueKind.Array)
                {
                    foreach (var g in genres.EnumerateArray()) if (g.TryGetProperty("name", out var name)) dto.Genres.Add(name.GetString() ?? string.Empty);
                }
                return dto;
            }

            // default to movie details
            return await GetMovieDetailsAsync(tmdbId);
        }

        public async Task<IEnumerable<MovieDto>> GetPopularMoviesAsync(int page = 1)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");

            var url = $"https://api.themoviedb.org/3/movie/popular?api_key={_apiKey}&language=en-US&page={page}";
            var res = await _http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            using var s = await res.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            var root = doc.RootElement;
            var results = new List<MovieDto>();
            if (root.TryGetProperty("results", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var m = new MovieDto
                    {
                        Id = item.GetProperty("id").GetInt32(),
                        Title = item.GetProperty("title").GetString() ?? string.Empty,
                        Overview = item.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                        PosterPath = item.TryGetProperty("poster_path", out var pp) && pp.GetString() is string p ? $"https://image.tmdb.org/t/p/w500{p}" : null
                    };
                    results.Add(m);
                }
            }

            return results;
        }

        public async Task<IEnumerable<MovieDto>> GetTrendingAsync(int page = 1)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");

            var url = $"https://api.themoviedb.org/3/trending/all/week?api_key={_apiKey}&language=en-US&page={page}";
            var res = await _http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            using var s = await res.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            var root = doc.RootElement;
            var results = new List<MovieDto>();
            if (root.TryGetProperty("results", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    // trending returns either 'title' (movie) or 'name' (tv)
                    var id = item.GetProperty("id").GetInt32();
                    var title = item.TryGetProperty("title", out var t) && t.GetString() is string ts && !string.IsNullOrWhiteSpace(ts)
                        ? ts
                        : item.TryGetProperty("name", out var n) && n.GetString() is string ns ? ns : string.Empty;

                    var m = new MovieDto
                    {
                        Id = id,
                        Title = title,
                        Overview = item.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                        PosterPath = item.TryGetProperty("poster_path", out var pp) && pp.GetString() is string p ? $"https://image.tmdb.org/t/p/w500{p}" : null
                    };

                    // set media type when available from trending result
                    if (item.TryGetProperty("media_type", out var mt)) m.MediaType = mt.GetString();

                    // release date may be 'release_date' (movie) or 'first_air_date' (tv)
                    if (item.TryGetProperty("release_date", out var rd) && DateTime.TryParse(rd.GetString(), out var d)) m.ReleaseDate = d;
                    else if (item.TryGetProperty("first_air_date", out var fa) && DateTime.TryParse(fa.GetString(), out var fd)) m.ReleaseDate = fd;

                    results.Add(m);
                }
            }

            return results;
        }

        public async Task<IEnumerable<RECO.Application.DTOs.VideoDto>> GetVideosAsync(int tmdbId, string mediaType)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) throw new InvalidOperationException("TMDB_API_KEY not configured");

            string url;
            if (string.Equals(mediaType, "tv", StringComparison.OrdinalIgnoreCase))
                url = $"https://api.themoviedb.org/3/tv/{tmdbId}/videos?api_key={_apiKey}&language=en-US";
            else
                url = $"https://api.themoviedb.org/3/movie/{tmdbId}/videos?api_key={_apiKey}&language=en-US";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDb videos request failed for {tmdbId} {mediaType} -> {status}", tmdbId, mediaType, res.StatusCode);
                return Array.Empty<RECO.Application.DTOs.VideoDto>();
            }

            using var s = await res.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            var root = doc.RootElement;
            var list = new List<RECO.Application.DTOs.VideoDto>();
            if (root.TryGetProperty("results", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var key = item.TryGetProperty("key", out var k) ? k.GetString() ?? string.Empty : string.Empty;
                    var site = item.TryGetProperty("site", out var s2) ? s2.GetString() ?? string.Empty : string.Empty;
                    var type = item.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty;
                    var name = item.TryGetProperty("name", out var n) ? n.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        list.Add(new RECO.Application.DTOs.VideoDto { Key = key, Site = site, Type = type, Name = name });
                    }
                }
            }

            return list;
        }
    }
}
