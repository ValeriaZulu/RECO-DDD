using System;
using System.Collections.Generic;
using System.Linq;
using RECO.Domain.Entities;
using RECO.Domain.ValueObjects;

namespace RECO.Domain.Aggregates
{
    /// <summary>
    /// Aggregate root that coordinates Title and Review behaviors.
    /// Ensures invariants such as preventing duplicate reviews for the same user and title.
    /// Contains pure domain logic and XML documentation for clarity.
    /// </summary>
    public class TitleAggregate
    {
        public Title Title { get; }

        public TitleAggregate(Title title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
        }

        /// <summary>
        /// Adds a new review to the title. Validates rating using MovieRating value object.
        /// Enforces that the same user cannot review the title twice.
        /// </summary>
        public Review AddReview(Guid reviewId, User user, int rating, string? text = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (Title.Reviews.Exists(r => r.UserId == user.Id))
            {
                throw new InvalidOperationException("User has already reviewed this title");
            }

            var movieRating = new MovieRating(rating);
            var review = new Review(reviewId, Title.Id, user.Id, movieRating.Value, text);
            Title.AddReview(review);
            return review;
        }

        /// <summary>
        /// Calculates the average rating for the title. Returns 0.0 if no reviews.
        /// </summary>
        public double CalculateAverageRating()
        {
            if (Title.Reviews == null || !Title.Reviews.Any()) return 0.0;
            return Title.Reviews.Average(r => r.Rating);
        }

        /// <summary>
        /// Retrieves all reviews for the title as a read-only list.
        /// </summary>
        public IReadOnlyList<Review> GetAllReviews() => Title.Reviews.AsReadOnly();
    }
}
