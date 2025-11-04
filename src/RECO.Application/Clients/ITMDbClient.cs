using System.Collections.Generic;
using System.Threading.Tasks;
using RECO.Application.DTOs;

namespace RECO.Application.Clients
{
    /// <summary>
    /// Abstraction for TMDb API client. Defined in Application so the Infrastructure can implement it (DIP).
    /// No HTTP logic here — implemented in Infrastructure.TMDbClient.TMDbAdapter.
    /// </summary>
    public interface ITMDbClient
    {
        Task<MovieDto> GetMovieDetailsAsync(int tmdbId);
        Task<MovieDto> GetDetailsAsync(int tmdbId, string? mediaType = null);
        Task<IEnumerable<MovieDto>> GetPopularMoviesAsync(int page = 1);
        Task<IEnumerable<RECO.Application.DTOs.VideoDto>> GetVideosAsync(int tmdbId, string mediaType);
        /// <summary>
        /// Returns a batch of trending titles (movies and series) — used by the worker to import multiple items.
        /// Maps TMDb responses into the common Application MovieDto shape.
        /// </summary>
        Task<IEnumerable<MovieDto>> GetTrendingAsync(int page = 1);
    }
}
