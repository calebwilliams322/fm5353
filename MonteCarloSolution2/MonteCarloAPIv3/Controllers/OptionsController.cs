using Microsoft.AspNetCore.Mvc;
using MonteCarloAPIv3.DTOs;
using MonteCarloAPIv3.Services;

namespace MonteCarloAPIv3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptionsController : ControllerBase
    {
        private readonly OptionService _service;
        private readonly PricingService _pricing;

        public OptionsController(OptionService service, PricingService pricing)
        {
            _service = service;
            _pricing = pricing;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var opt = _service.Get(id);
            return opt == null ? NotFound() : Ok(opt);
        }

        [HttpPost]
        public IActionResult Create([FromBody] OptionDto dto)
        {
            var created = _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] OptionDto dto)
            => _service.Update(id, dto) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
            => _service.Delete(id) ? NoContent() : NotFound();

        [HttpPost("{id}/price")]
        public IActionResult PriceSavedOption(int id, [FromBody] PricingRequestDto settings)
        {
            var opt = _service.Get(id);
            if (opt == null) return NotFound();
            settings.Option = opt;
            return Ok(_pricing.PriceOption(settings));
        }
    }
}