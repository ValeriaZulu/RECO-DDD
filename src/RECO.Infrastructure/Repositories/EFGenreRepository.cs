using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;
using RECO.Infrastructure.Persistence;

namespace RECO.Infrastructure.Repositories
{
    public class EFGenreRepository : IGenreRepository
    {
        private readonly RECODbContext _db;

        public EFGenreRepository(RECODbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Genre?> GetByNameAsync(string name)
        {
            return await _db.Set<Genre>().FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task AddAsync(Genre genre)
        {
            if (genre == null) throw new ArgumentNullException(nameof(genre));
            await _db.Set<Genre>().AddAsync(genre);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _db.Set<Genre>().ToListAsync();
        }
    }
}
