using Microsoft.AspNetCore.Mvc;
using OneSms.Services;

namespace OneSms.Online.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private TimService _timService;

        public AdminController(TimService timService)
        {
            _timService = timService;
        }

        [HttpGet("start")]
        public IActionResult StartTimerService()
        {
            _timService.StartTimer();
            return Ok("started");
        }
    }
}
