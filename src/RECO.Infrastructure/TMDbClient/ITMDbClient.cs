using System.Collections.Generic;
using System.Threading.Tasks;

namespace RECO.Infrastructure.TMDbClient
{
    public interface ITMDbClient
    {
        Task<IEnumerable<TMDbDto>> GetPopularMoviesAsync(int page = 1);
        Task<TMDbDto?> GetDetailsAsync(int tmdbId, string type = "movie");
    }

    public record TMDbDto(int Id, string Title, string? Overview, string? PosterPath);
}
