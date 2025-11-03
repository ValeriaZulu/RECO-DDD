using System;
using System.Collections.Generic;

namespace RECO.Domain.Entities
{
    public enum TitleType { Movie, Series }

    public class Title
    {
        public Guid Id { get; private set; }
        public int TmdbId { get; private set; }
        public TitleType Type { get; private set; }
        public string TitleName { get; private set; } = string.Empty;
        public string? Synopsis { get; private set; }
        public string? PosterUrl { get; private set; }
        public DateTime? ReleaseDate { get; private set; }

        public List<Genre> Genres { get; private set; } = new();
        public List<PlatformAvailability> Availabilities { get; private set; } = new();
    // Reviews are part of the Title aggregate. Title is the aggregate root and
    // must manage adding reviews to preserve invariants (see constitution: "aggregate roots control consistency").
    public List<Review> Reviews { get; private set; } = new();

        private Title() { }

        public Title(Guid id, int tmdbId, TitleType type, string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("title");
            Id = id;
            TmdbId = tmdbId;
            Type = type;
            TitleName = title;
        }

        /// <summary>
        /// Adds a review to the Title aggregate. Aggregate root enforces that the review's TitleId matches.
        /// Uses SRP and guards invariants (see constitution).
        /// </summary>
        public void AddReview(Review review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));
            if (review.TitleId != Id) throw new InvalidOperationException("Review TitleId does not match aggregate root Id");
            Reviews.Add(review);
        }

        public void SetSynopsis(string synopsis) => Synopsis = synopsis;
        public void SetPoster(string url) => PosterUrl = url;
        public void AddGenre(Genre g) => Genres.Add(g);
        public void AddAvailability(PlatformAvailability p) => Availabilities.Add(p);
    }

    public class Genre
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        private Genre() { }
        public Genre(int id, string name)
        {
            Id = id; Name = name;
        }
    }

    public class PlatformAvailability
    {
        public Guid Id { get; private set; }
        public string ProviderName { get; private set; } = string.Empty;
        public string? Url { get; private set; }
        private PlatformAvailability() { }
        public PlatformAvailability(string providerName, string? url)
        {
            Id = Guid.NewGuid();
            ProviderName = providerName;
            Url = url;
        }
    }
}
