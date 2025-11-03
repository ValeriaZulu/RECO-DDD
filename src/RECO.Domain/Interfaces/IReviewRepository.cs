using System;
using System.Threading.Tasks;
using RECO.Domain.Entities;

namespace RECO.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task<Review?> GetByIdAsync(Guid id);
    }
}
