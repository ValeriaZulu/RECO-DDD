using System;

namespace RECO.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; private set; }
        public Guid TitleId { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string? Text { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Review() { }

        public Review(Guid id, Guid titleId, Guid userId, int rating, string? text = null)
        {
            if (rating < 1 || rating > 10) throw new ArgumentOutOfRangeException(nameof(rating));
            Id = id; TitleId = titleId; UserId = userId; Rating = rating; Text = text; CreatedAt = DateTime.UtcNow;
        }
    }
}
