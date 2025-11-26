using Microsoft.AspNetCore.Mvc;
using MonteCarloAPI.Models;
using MonteCarloAPI.Services;

namespace MonteCarloAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptionsController : ControllerBase
    {
        private readonly OptionService _optionService;

        public OptionsController(OptionService optionService)
        {
            _optionService = optionService;
        }

        // POST: /api/options
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOption([FromBody] OptionConfigDTO newOption)
        {
            if (newOption?.OptionParameters == null)
                return BadRequest(new { message = "OptionParameters are required." });

            if (newOption.StockId <= 0)
                return BadRequest(new { message = "StockId is required and must be greater than 0." });

            try
            {
                var created = await _optionService.AddOptionAsync(newOption);
                return CreatedAtAction(nameof(GetOptionById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /api/options/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOptionById(int id)
        {
            var option = await _optionService.GetOptionByIdAsync(id);
            if (option == null)
                return NotFound(new { message = $"Option with ID {id} not found." });

            return Ok(option);
        }

        // GET: /api/options
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOptions()
        {
            var options = await _optionService.GetAllOptionsAsync();
            return Ok(new { count = options.Count, options });
        }

        // PUT: /api/options/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOption(int id, [FromBody] OptionConfigDTO updatedOption)
        {
            if (updatedOption?.OptionParameters == null)
                return BadRequest(new { message = "OptionParameters are required." });

            if (updatedOption.StockId <= 0)
                return BadRequest(new { message = "StockId is required and must be greater than 0." });

            try
            {
                var result = await _optionService.UpdateOptionAsync(id, updatedOption);
                if (result == null)
                    return NotFound(new { message = $"Option with ID {id} not found." });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: /api/options/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var success = await _optionService.DeleteOptionAsync(id);
            if (!success)
                return NotFound(new { message = $"Option with ID {id} not found." });

            return NoContent();
        }
    }
}
