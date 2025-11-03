using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RECO.Domain.Entities;

namespace RECO.Domain.Interfaces
{
    public interface ITitleRepository
    {
        // Keep interfaces small and intent-revealing (ISP per constitution)
        Task<Title?> GetByTmdbIdAsync(int tmdbId);
        Task UpsertAsync(Title title);
        Task<IEnumerable<Title>> SearchByGenreAsync(int genreId, int limit = 50);
    }
}
