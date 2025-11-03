using System;
using System.Threading.Tasks;
using RECO.Domain.Entities;

namespace RECO.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }
}
