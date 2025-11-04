using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RECO.Application.Clients;
using RECO.Application.DTOs;
using RECO.Application.Services;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;
using Xunit;

namespace RECO.Application.Tests
{
    public class ImportServiceTests
    {
        [Fact]
        public async Task ImportFromTMDbAsync_MapsMovieToTitleAndCallsRepository()
        {
            // Arrange
            var tmdbId = 12345;
            var movie = new MovieDto
            {
                Id = tmdbId,
                Title = "The Test Movie",
                Overview = "A test overview",
                PosterPath = "https://image.tmdb.org/t/p/w500/test.jpg",
                ReleaseDate = new DateTime(2020,1,2)
            };

            var mockTmdb = new Mock<ITMDbClient>();
            mockTmdb.Setup(m => m.GetMovieDetailsAsync(tmdbId)).ReturnsAsync(movie);

            var mockRepo = new Mock<ITitleRepository>();
            Title? captured = null;
            mockRepo.Setup(r => r.UpsertAsync(It.IsAny<Title>()))
                .Callback<Title>(t => captured = t)
                .Returns(Task.CompletedTask);

            var svc = new ImportService(mockTmdb.Object, mockRepo.Object);

            // Act
            await svc.ImportFromTMDbAsync(tmdbId);

            // Assert
            mockTmdb.Verify(m => m.GetMovieDetailsAsync(tmdbId), Times.Once);
            mockRepo.Verify(r => r.UpsertAsync(It.IsAny<Title>()), Times.Once);

            captured.Should().NotBeNull();
            captured!.TmdbId.Should().Be(movie.Id);
            captured.TitleName.Should().Be(movie.Title);
            captured.Synopsis.Should().Be(movie.Overview);
            captured.PosterUrl.Should().Be(movie.PosterPath);
            captured.ReleaseDate.Should().Be(movie.ReleaseDate);
        }
    }
}
