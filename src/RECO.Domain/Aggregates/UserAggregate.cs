using System;
using System.Collections.Generic;
using System.Linq;
using RECO.Domain.Entities;
using RECO.Domain.ValueObjects;

namespace RECO.Domain.Aggregates
{
    /// <summary>
    /// Aggregate root that coordinates User, Profile and Review behaviors.
    /// Ensures invariants: a user can only have one review per title.
    /// This class contains only domain logic and does not depend on infra or application concerns (per constitution).
    /// </summary>
    public class UserAggregate
    {
        public User User { get; }

        public UserAggregate(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        /// <summary>
        /// Adds or updates the user's profile. Delegates to the User entity to set the Profile.
        /// </summary>
        public void AddOrUpdateProfile(Profile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            User.SetProfile(profile);
        }

        /// <summary>
        /// Adds a review from this user to the specified title.
        /// Enforces the invariant that a user cannot create more than one review per title.
        /// Returns the created Review instance.
        /// </summary>
        public Review AddReviewToTitle(Title title, Guid reviewId, int rating, string? text = null)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));

            if (title.Reviews.Exists(r => r.UserId == User.Id))
            {
                throw new InvalidOperationException("User has already reviewed this title");
            }

            // Use domain VO to validate rating rules
            var movieRating = new MovieRating(rating);

            var review = new Review(reviewId, title.Id, User.Id, movieRating.Value, text);
            title.AddReview(review);
            return review;
        }

        /// <summary>
        /// Retrieves review history for this user from a set of titles.
        /// Caller provides titles to keep Domain isolated from persistence concerns.
        /// </summary>
        public IEnumerable<Review> GetReviewHistory(IEnumerable<Title> titles)
        {
            if (titles == null) throw new ArgumentNullException(nameof(titles));
            return titles.SelectMany(t => t.Reviews).Where(r => r.UserId == User.Id);
        }
    }
}
