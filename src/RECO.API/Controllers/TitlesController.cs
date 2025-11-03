using Microsoft.AspNetCore.Mvc;

namespace RECO.API.Controllers
{
    public class TitlesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.TmdbId = id;
            return View();
        }
    }
}
