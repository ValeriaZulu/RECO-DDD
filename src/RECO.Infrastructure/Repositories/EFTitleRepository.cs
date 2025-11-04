using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;
using RECO.Infrastructure.Persistence;

namespace RECO.Infrastructure.Repositories
{
    /// <summary>
    /// EF implementation of ITitleRepository. Repository implementation in Infrastructure - implements Domain ITitleRepository (DIP per constitution).
    /// Responsible for loading and persisting Title aggregates.
    /// </summary>
    public class EFTitleRepository : ITitleRepository
    {
        private readonly RECODbContext _db;

        public EFTitleRepository(RECODbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IEnumerable<Title>> SearchByGenreAsync(int genreId, int limit = 50)
        {
            return await _db.Titles
                .Where(t => t.Genres.Any(g => g.Id == genreId))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Title?> GetByTmdbIdAsync(int tmdbId)
        {
            return await _db.Titles.FirstOrDefaultAsync(t => EF.Property<int>(t, "TmdbId") == tmdbId);
        }

        public async Task<Title?> GetByIdAsync(Guid id)
        {
            // Load the Title first, then explicitly load related collections to ensure the TitleGenres join is used.
            var title = await _db.Titles.FirstOrDefaultAsync(t => t.Id == id);
            if (title == null) return null;

            // Explicitly load genres and reviews to ensure navigations are populated
            await _db.Entry(title).Collection(t => t.Genres).LoadAsync();
            await _db.Entry(title).Collection(t => t.Reviews).LoadAsync();

            return title;
        }

        public async Task UpsertAsync(Title title)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            // Merge pattern: load existing aggregate and merge children to avoid update conflicts
            var existing = await _db.Titles.Include(t => t.Reviews).FirstOrDefaultAsync(t => t.Id == title.Id);
            if (existing == null)
            {
                await _db.Titles.AddAsync(title);
                await _db.SaveChangesAsync();
                return;
            }

            // Simpler and safer: remove existing aggregate and insert the provided one.
            // This avoids complex merge semantics in this exercise and keeps persistence details inside Infrastructure (constitution: SRP/DIP).
            _db.Titles.Remove(existing);
            await _db.SaveChangesAsync();

            await _db.Titles.AddAsync(title);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Title>> GetAllAsync()
        {
            return await _db.Titles
                .Include(t => t.Genres)
                .Include(t => t.Reviews)
                .ToListAsync();
        }

        public async Task<IEnumerable<Title>> GetByTypeAsync(TitleType type)
        {
            return await _db.Titles
                .Where(t => t.Type == type)
                .Include(t => t.Genres)
                .Include(t => t.Reviews)
                .ToListAsync();
        }
    }
}
