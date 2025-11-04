using System;
using System.Threading.Tasks;
using RECO.Application.Clients;
using RECO.Application.Interfaces;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;

namespace RECO.Application.Services
{
    /// <summary>
    /// Application service that imports movie data from TMDb and persists it using domain repositories.
    /// Follows DDD: depends on an abstraction for the external API (ITMDbClient) and repository interfaces (DIP).
    /// </summary>
    public class ImportService : IImportService
    {
        private readonly ITMDbClient _tmdb;
        private readonly ITitleRepository _titleRepository;
        private readonly RECO.Domain.Interfaces.IGenreRepository _genreRepository;

        public ImportService(ITMDbClient tmdb, ITitleRepository titleRepository, RECO.Domain.Interfaces.IGenreRepository genreRepository)
        {
            _tmdb = tmdb ?? throw new ArgumentNullException(nameof(tmdb));
            _titleRepository = titleRepository ?? throw new ArgumentNullException(nameof(titleRepository));
            _genreRepository = genreRepository ?? throw new ArgumentNullException(nameof(genreRepository));
        }

        public async Task ImportFromTMDbAsync(int tmdbId)
        {
            var movie = await _tmdb.GetMovieDetailsAsync(tmdbId);
            if (movie == null) throw new InvalidOperationException($"Movie with id {tmdbId} not found on TMDb");

            // Check existing title by TMDb id to perform idempotent upsert
            var existing = await _titleRepository.GetByTmdbIdAsync(movie.Id);
            if (existing is null)
            {
                var title = new Title(Guid.NewGuid(), movie.Id, TitleType.Movie, movie.Title ?? "Untitled");
                if (!string.IsNullOrWhiteSpace(movie.Overview)) title.SetSynopsis(movie.Overview);
                if (!string.IsNullOrWhiteSpace(movie.PosterPath)) title.SetPoster(movie.PosterPath);
                if (movie.ReleaseDate != DateTime.MinValue) title.SetReleaseDate(movie.ReleaseDate);

                // Persist genres: ensure Genre entities exist and associate them with the title
                foreach (var genreName in movie.Genres)
                {
                    if (string.IsNullOrWhiteSpace(genreName)) continue;
                    var g = await _genreRepository.GetByNameAsync(genreName);
                    if (g == null)
                    {
                        g = new Genre(genreName);
                        await _genreRepository.AddAsync(g);
                    }
                    title.AddGenre(g);
                }

                await _titleRepository.UpsertAsync(title);
            }
            else
            {
                // Update fields on existing aggregate and persist
                if (!string.IsNullOrWhiteSpace(movie.Title) && existing.TitleName != movie.Title)
                    existing.Rename(movie.Title);

                if (!string.IsNullOrWhiteSpace(movie.Overview)) existing.SetSynopsis(movie.Overview);
                if (!string.IsNullOrWhiteSpace(movie.PosterPath)) existing.SetPoster(movie.PosterPath);
                if (movie.ReleaseDate != DateTime.MinValue) existing.SetReleaseDate(movie.ReleaseDate);

                // Ensure genres are associated
                foreach (var genreName in movie.Genres)
                {
                    if (string.IsNullOrWhiteSpace(genreName)) continue;
                    var g = await _genreRepository.GetByNameAsync(genreName);
                    if (g == null)
                    {
                        g = new Genre(genreName);
                        await _genreRepository.AddAsync(g);
                    }
                    if (!existing.Genres.Exists(x => x.Name == g.Name)) existing.AddGenre(g);
                }

                await _titleRepository.UpsertAsync(existing);
            }
        }

        // Title renaming is handled by the domain's Rename() method to avoid re-creation.
    }
}
