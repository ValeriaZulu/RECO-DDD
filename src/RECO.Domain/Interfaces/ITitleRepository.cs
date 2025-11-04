using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RECO.Domain.Entities;

namespace RECO.Domain.Interfaces
{
    public interface ITitleRepository
    {
        // Keep interfaces small and intent-revealing (ISP per constitution)
        // Added GetByIdAsync to allow aggregate retrieval by GUID (DIP: repository interface, not EF types)
        // This follows the project's constitution rule to keep DI on interfaces and maintain SRP.
        Task<Title?> GetByIdAsync(Guid id);
        Task<Title?> GetByTmdbIdAsync(int tmdbId);
        Task UpsertAsync(Title title);
        Task<IEnumerable<Title>> SearchByGenreAsync(int genreId, int limit = 50);
        Task<IEnumerable<Title>> GetAllAsync();
        Task<IEnumerable<Title>> GetByTypeAsync(TitleType type);
    }
}
