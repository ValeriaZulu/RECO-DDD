using System;

namespace RECO.Domain.ValueObjects
{
    /// <summary>
    /// Immutable MovieRating value object with validation (1-10 scale).
    /// </summary>
    public sealed class MovieRating
    {
        public int Value { get; }

        public MovieRating(int value)
        {
            if (value < 1 || value > 10) throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 1 and 10");
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}
