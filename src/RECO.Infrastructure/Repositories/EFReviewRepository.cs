using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;
using RECO.Infrastructure.Persistence;

namespace RECO.Infrastructure.Repositories
{
    /// <summary>
    /// EF implementation of repository operations for Review. Kept in Infrastructure per constitution (DIP).
    /// </summary>
    public class EFReviewRepository : IReviewRepository
    {
        private readonly RECODbContext _db;

        public EFReviewRepository(RECODbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task AddAsync(Review review)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));
            await _db.Reviews.AddAsync(review);
            await _db.SaveChangesAsync();
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            return await _db.Reviews.FindAsync(id);
        }
    }
}
