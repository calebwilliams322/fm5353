using Microsoft.AspNetCore.Mvc;
using MonteCarloAPIv3.DTOs;
using MonteCarloAPIv3.Services;

namespace MonteCarloAPIv3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly PricingService _pricing;

        public PricingController(PricingService pricing)
        {
            _pricing = pricing;
        }

        [HttpPost]
        public IActionResult PriceAdHoc([FromBody] PricingRequestDto req)
        {
            var result = _pricing.PriceOption(req);
            return Ok(result);
        }
    }
}