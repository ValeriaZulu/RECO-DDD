using System;
using System.Collections.Generic;
using System.Linq;
using RECO.Domain.Entities;

namespace RECO.Domain.Services
{
    /// <summary>
    /// Simple recommendation service contained in Domain for business logic of recommending titles.
    /// This service is domain-centric and intentionally does not depend on infra or external APIs.
    /// </summary>
    public class RecommendationService
    {
        /// <summary>
        /// Recommend top N titles by average rating.
        /// </summary>
        public IEnumerable<Title> RecommendByAverageRating(IEnumerable<Title> candidates, int topN = 10)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            return candidates
                .Select(t => new { Title = t, Avg = t.Reviews.Any() ? t.Reviews.Average(r => r.Rating) : 0.0 })
                .OrderByDescending(x => x.Avg)
                .ThenBy(x => x.Title.TitleName)
                .Take(topN)
                .Select(x => x.Title);
        }

        /// <summary>
        /// Recommend titles that match the user's genre preferences first.
        /// </summary>
        public IEnumerable<Title> RecommendByProfile(IEnumerable<Title> candidates, Profile profile, int topN = 10)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            if (profile == null) return RecommendByAverageRating(candidates, topN);

            var preferred = new HashSet<int>(profile.GenrePreferences.Select(g => g.GenreId));

            return candidates
                .Select(t => new { Title = t, Score = (t.Genres != null && t.Genres.Any(g => preferred.Contains(g.Id)) ? 1 : 0) , Avg = t.Reviews.Any() ? t.Reviews.Average(r => r.Rating) : 0.0 })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Avg)
                .ThenBy(x => x.Title.TitleName)
                .Take(topN)
                .Select(x => x.Title);
        }
    }
}
