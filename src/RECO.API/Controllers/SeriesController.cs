using Microsoft.AspNetCore.Mvc;
using RECO.Domain.Entities;
using RECO.Domain.Interfaces;

namespace RECO.API.Controllers
{
    public class SeriesController : Controller
    {
        private readonly ITitleRepository _titleRepository;

        public SeriesController(ITitleRepository titleRepository)
        {
            _titleRepository = titleRepository;
        }

        public async Task<IActionResult> Index()
        {
            var series = await _titleRepository.GetByTypeAsync(TitleType.Series) ?? new List<Title>();
            ViewData["Title"] = "Series";
            return View("~/Views/Titles/Index.cshtml", series);
        }

    }
}
