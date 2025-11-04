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

        // Backing field that always stores UTC-kind DateTime
        private DateTime _releaseDate;
        public DateTime ReleaseDate
        {
            get => _releaseDate;
            private set => _releaseDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public List<Genre> Genres { get; private set; } = new();
        public List<PlatformAvailability> Availabilities { get; private set; } = new();

        // Reviews are part of the Title aggregate.
        public List<Review> Reviews { get; private set; } = new();

        private Title() { }

        public Title(Guid id, int tmdbId, TitleType type, string title)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("title");
            Id = id;
            TmdbId = tmdbId;
            Type = type;
            TitleName = title;
            // Initialize ReleaseDate with a known UTC default if needed
            _releaseDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        }

        /// <summary>
        /// Sets the release date safely from a nullable DateTime (e.g. DTO input).
        /// If null, leaves the ReleaseDate unchanged.
        /// The stored DateTime will have Kind = Utc.
        /// </summary>
        public void SetReleaseDate(DateTime? date)
        {
            if (date.HasValue)
            {
                ReleaseDate = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
            }
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
        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException(nameof(newName));
            TitleName = newName;
        }
        public void AddGenre(Genre g) => Genres.Add(g);
        public void AddAvailability(PlatformAvailability p) => Availabilities.Add(p);
        /// <summary>
        /// Update the TitleType (e.g., when importing metadata that clarifies movie vs series).
        /// </summary>
        public void SetType(TitleType type) => Type = type;
    }

    public class Genre
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // EF Core requires a parameterless constructor for materialization
        protected Genre() { }

        // Create a new Genre with a name; Id will be assigned by the database
        public Genre(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Name = name;
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
