using System;
using MediatR;
using RECO.Application.DTOs;

namespace RECO.Application.Commands
{
    /// <summary>
    /// Command to create a Review. Uses CQRS pattern via MediatR (registered in API).
    /// </summary>
    public class CreateReviewCommand : IRequest<Guid>
    {
        public ReviewDto Review { get; }

        public CreateReviewCommand(ReviewDto review)
        {
            Review = review ?? throw new ArgumentNullException(nameof(review));
        }
    }
}
