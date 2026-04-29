using Microsoft.AspNetCore.Mvc;
using SavePoint.BusinessLogic.Services.Interfaces;

namespace SavePoint.LookupService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("genres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _lookupService.GetGenresAsync();
            return Ok(genres);
        }

        [HttpGet("platforms")]
        public async Task<IActionResult> GetPlatforms()
        {
            var platforms = await _lookupService.GetPlatformsAsync();
            return Ok(platforms);
        }

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _lookupService.GetCompaniesAsync();
            return Ok(companies);
        }
    }
}
