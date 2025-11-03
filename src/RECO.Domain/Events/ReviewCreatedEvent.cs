using System;

namespace RECO.Domain.Events
{
    /// <summary>
    /// Domain event published when a Review is created.
    /// Part of the Domain layer so other layers can subscribe via interfaces (DIP per constitution).
    /// </summary>
    public sealed class ReviewCreatedEvent
    {
        public Guid ReviewId { get; }
        public Guid TitleId { get; }
        public Guid UserId { get; }
        public int Rating { get; }
        public DateTime CreatedAt { get; }

        public ReviewCreatedEvent(Guid reviewId, Guid titleId, Guid userId, int rating, DateTime createdAt)
        {
            ReviewId = reviewId;
            TitleId = titleId;
            UserId = userId;
            Rating = rating;
            CreatedAt = createdAt;
        }
    }
}
