using System.Threading.Tasks;

namespace RECO.Application.Interfaces
{
    public interface IImportService
    {
        Task ImportFromTMDbAsync(int tmdbId);
    }
}
