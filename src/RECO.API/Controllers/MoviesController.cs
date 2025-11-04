using Microsoft.AspNetCore.Mvc;
using RECO.Domain.Interfaces;
using RECO.Domain.Entities;
using System.Threading.Tasks;

namespace RECO.API.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ITitleRepository _titleRepository;

        public MoviesController(ITitleRepository titleRepository)
        {
            _titleRepository = titleRepository;
        }

            public async Task<IActionResult> Index()
            {
                var movies = await _titleRepository.GetByTypeAsync(TitleType.Movie) ?? new List<Title>();
                ViewData["Title"] = "Movies";
                return View("~/Views/Titles/Index.cshtml", movies);
            }

    }
}
