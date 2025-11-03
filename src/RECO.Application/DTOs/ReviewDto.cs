using System;

namespace RECO.Application.DTOs
{
    /// <summary>
    /// Data transfer object for creating reviews.
    /// Keeps transport shape separate from domain entity (SRP, see constitution).
    /// </summary>
    public class ReviewDto
    {
        public Guid TitleId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Content { get; set; }
    }
}
