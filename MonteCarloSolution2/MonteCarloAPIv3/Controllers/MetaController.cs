using Microsoft.AspNetCore.Mvc;

namespace MonteCarloAPIv3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetaController : ControllerBase
    {
        [HttpGet("option-types")]
        public IActionResult GetOptionTypes() => Ok(new[]
        {
            "European", "Asian", "Digital", "Barrier", "Lookback", "Range"
        });

        [HttpGet("simulation-modes")]
        public IActionResult GetSimModes() => Ok(new[]
        {
            "Plain", "Antithetic", "ControlVariate", "AntitheticAndControlVariate", "VanDerCorput"
        });
    }
}