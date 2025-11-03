using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RECO.Application.Commands;
using RECO.Application.DTOs;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;
using RECO.Domain.Events;
using RECO.Domain.ValueObjects;

namespace RECO.Application.Handlers
{
    /// <summary>
    /// Handles CreateReviewCommand. Uses DIP: depends on repository interfaces not EF types (constitution).
    /// Follows SRP: this handler only orchestrates creation of a Review and publishing the domain event.
    /// </summary>
    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Guid>
    {
        private readonly ITitleRepository _titleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDomainEventDispatcher _dispatcher;

        public CreateReviewHandler(ITitleRepository titleRepository, IUserRepository userRepository, IDomainEventDispatcher dispatcher)
        {
            _titleRepository = titleRepository ?? throw new ArgumentNullException(nameof(titleRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Review ?? throw new ArgumentNullException(nameof(request.Review));

            // Ensure user exists before creating review
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null) throw new InvalidOperationException("User not found");

            // Load title aggregate
            var title = await _titleRepository.GetByIdAsync(dto.TitleId);
            if (title == null) throw new InvalidOperationException("Title not found");

            // Use domain ValueObject to validate rating rules (1-10)
            var rating = new MovieRating(dto.Rating);

            // Create domain Review entity and add via aggregate root (Title)
            var review = new Review(Guid.NewGuid(), dto.TitleId, dto.UserId, rating.Value, dto.Content);
            title.AddReview(review);

            // Persist aggregate via repository (DIP to ITitleRepository; infra implements persistence)
            await _titleRepository.UpsertAsync(title);

            // Publish domain event so other bounded contexts or handlers can react
            var evt = new ReviewCreatedEvent(review.Id, review.TitleId, review.UserId, review.Rating, review.CreatedAt);
            await _dispatcher.PublishAsync(evt);

            return review.Id;
        }
    }
}
