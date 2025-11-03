using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;
using RECO.Infrastructure.Persistence;

namespace RECO.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation in Infrastructure - implements Domain IUserRepository, per DIP (constitution).
    /// Keeps persistence concerns isolated in Infrastructure (Clean Architecture).
    /// </summary>
    public class EFUserRepository : IUserRepository
    {
        private readonly RECODbContext _db;

        public EFUserRepository(RECODbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task AddAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users.FindAsync(id);
        }
    }
}
