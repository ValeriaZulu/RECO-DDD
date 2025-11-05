using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RECO.Domain.Interfaces;

namespace RECO.API.Controllers.Api
{
    [ApiController]
    [Route("api/genres")]
    public class GenresApiController : ControllerBase
    {
        private readonly RECO.Domain.Interfaces.IGenreRepository _genres;
        public GenresApiController(RECO.Domain.Interfaces.IGenreRepository genres)
        {
            _genres = genres;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _genres.GetAllAsync();
            return Ok(list);
        }
    }
}
