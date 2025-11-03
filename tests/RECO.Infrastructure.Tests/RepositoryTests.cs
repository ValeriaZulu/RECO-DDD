using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RECO.Domain.Entities;
using RECO.Infrastructure.Persistence;
using RECO.Infrastructure.Repositories;
using Xunit;

namespace RECO.Infrastructure.Tests
{
    public class RepositoryTests
    {
        private RECODbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<RECODbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new RECODbContext(options);
        }

        [Fact]
        public async Task EFUserRepository_AddsAndRetrievesUser()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);
            var repo = new EFUserRepository(ctx);

            var user = new User(Guid.NewGuid(), "test@x.com", "hash");
            await repo.AddAsync(user);

            var fetched = await repo.GetByIdAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.Equal(user.Email, fetched!.Email);
        }

        [Fact]
        public async Task EFTitleRepository_UpsertAndGetById_PreservesReviews()
        {
            var dbName = Guid.NewGuid().ToString();
            using var ctx = CreateContext(dbName);
            var repo = new EFTitleRepository(ctx);

            var titleId = Guid.NewGuid();
            var title = new Title(titleId, 123, TitleType.Movie, "T");
            await repo.UpsertAsync(title);

            var fetched = await repo.GetByIdAsync(titleId);
            Assert.NotNull(fetched);
            Assert.Equal(titleId, fetched!.Id);

            // Simulate a different unit-of-work: create a new Title instance with the same Id and a new review,
            // then call UpsertAsync to ensure repository merge logic inserts the new child.
            var review = new Review(Guid.NewGuid(), titleId, Guid.NewGuid(), 7, "Nice");
            var updatedTitle = new Title(titleId, 123, TitleType.Movie, "T");
            updatedTitle.AddReview(review);
            await repo.UpsertAsync(updatedTitle);

            var fetched2 = await repo.GetByIdAsync(titleId);
            Assert.NotNull(fetched2);
            Assert.Contains(fetched2!.Reviews, r => r.Id == review.Id && r.Rating == 7);
        }
    }
}
