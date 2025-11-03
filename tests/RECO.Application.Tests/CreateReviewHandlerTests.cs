using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RECO.Application.Commands;
using RECO.Application.DTOs;
using RECO.Application.Handlers;
using RECO.Domain.Entities;
using RECO.Domain.Events;
using RECO.Domain.Interfaces;
using Xunit;

namespace RECO.Application.Tests
{
    public class CreateReviewHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidDto_AddsReviewAndPublishesEvent()
        {
            // Arrange
            var titleId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var mockTitleRepo = new Mock<ITitleRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockDispatcher = new Mock<IDomainEventDispatcher>();

            var title = new Title(titleId, 999, RECO.Domain.Entities.TitleType.Movie, "Test Movie");
            mockTitleRepo.Setup(r => r.GetByIdAsync(titleId)).ReturnsAsync(title);
            mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User(userId, "a@b.com", "hash"));

            ReviewDto dto = new ReviewDto { TitleId = titleId, UserId = userId, Rating = 8, Content = "Great" };
            var cmd = new CreateReviewCommand(dto);

            var handler = new CreateReviewHandler(mockTitleRepo.Object, mockUserRepo.Object, mockDispatcher.Object);

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            mockTitleRepo.Verify(r => r.UpsertAsync(It.Is<Title>(t => t.Reviews.Exists(rv => rv.UserId == userId && rv.Rating == 8))), Times.Once);
            mockDispatcher.Verify(d => d.PublishAsync(It.IsAny<ReviewCreatedEvent>()), Times.Once);
        }
    }
}
