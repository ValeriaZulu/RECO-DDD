using System;
using RECO.Domain.Entities;

namespace RECO.Domain.Aggregates
{
    /// <summary>
    /// ReviewAggregate groups User, Title and Review entities to enforce cross-entity invariants.
    /// Keeps consistency rules inside the Domain layer (no infra or app dependencies) per the constitution.
    /// </summary>
    public class ReviewAggregate
    {
        public User User { get; }
        public Title Title { get; }

        public ReviewAggregate(User user, Title title)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Title = title ?? throw new ArgumentNullException(nameof(title));
        }

        /// <summary>
        /// Ensure the user has not already created a review for this title.
        /// Throws InvalidOperationException if duplicate found.
        /// </summary>
        public void EnsureNoDuplicateReview()
        {
            if (Title.Reviews.Exists(r => r.UserId == User.Id))
            {
                throw new InvalidOperationException("User has already created a review for this title");
            }
        }

        /// <summary>
        /// Adds a review to the aggregate after enforcing invariants.
        /// Title (aggregate root) receives the review to maintain consistency.
        /// </summary>
        public Review AddReview(Guid id, int rating, string? text = null)
        {
            EnsureNoDuplicateReview();
            var review = new Review(id, Title.Id, User.Id, rating, text);
            Title.AddReview(review);
            return review;
        }
    }
}
