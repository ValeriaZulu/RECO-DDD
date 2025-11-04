using System.Threading.Tasks;
using RECO.Domain.Entities;

namespace RECO.Domain.Interfaces
{
    public interface IGenreRepository
    {
        Task<Genre?> GetByNameAsync(string name);
        Task AddAsync(Genre genre);
    }
}
