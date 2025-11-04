using Microsoft.AspNetCore.Mvc;
using RECO.Domain.Interfaces;
using RECO.Domain.Entities;

namespace RECO.API.Controllers
{
    public class TitlesController : Controller
    {
        private readonly ITitleRepository _titleRepository;
        private readonly RECO.Application.Clients.ITMDbClient _tmdb;

        public TitlesController(ITitleRepository titleRepository, RECO.Application.Clients.ITMDbClient tmdb)
        {
            _titleRepository = titleRepository;
            _tmdb = tmdb;
        }

        public async Task<IActionResult> Details(System.Guid id)
        {
            if (id == System.Guid.Empty) return NotFound();
            var title = await _titleRepository.GetByIdAsync(id);
            if (title == null) return NotFound();

            // Attempt to fetch TMDb videos for an embedded trailer
            try
            {
                var mediaType = title.Type == RECO.Domain.Entities.TitleType.Series ? "tv" : "movie";
                var videos = await _tmdb.GetVideosAsync(title.TmdbId, mediaType);
                // pick the first YouTube trailer
                var trailer = videos.FirstOrDefault(v => string.Equals(v.Site, "YouTube", StringComparison.OrdinalIgnoreCase)
                                                         && v.Type?.IndexOf("trailer", StringComparison.OrdinalIgnoreCase) >= 0);
                if (trailer != null)
                {
                    ViewBag.TrailerKey = trailer.Key;
                }
            }
            catch (Exception ex)
            {
                // don't fail the page if TMDb call fails; log and continue
                // view will show a fallback link
                Console.WriteLine(ex.Message);
            }

            return View("~/Views/Titles/Details.cshtml", title);
        }
    }

}
