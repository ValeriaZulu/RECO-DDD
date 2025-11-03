using Microsoft.AspNetCore.Mvc;

namespace RECO.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy() => View();
    }
}
