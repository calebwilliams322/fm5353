using Microsoft.AspNetCore.Mvc;
using MonteCarloAPI.Models;
using MonteCarloAPI.Services;

namespace MonteCarloAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly OptionService _optionService;
        private readonly PricingService _pricingService;

        public PricingController(OptionService optionService, PricingService pricingService)
        {
            _optionService = optionService;
            _pricingService = pricingService;
        }

        // POST: /api/pricing/{optionId}
        [HttpPost("{optionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PriceOption(int optionId, [FromBody] SimulationParametersDTO simParams)
        {
            if (simParams == null)
                return BadRequest(new { message = "SimulationParameters are required." });

            // Get the option configuration
            var option = await _optionService.GetOptionByIdAsync(optionId);
            if (option == null)
                return NotFound(new { message = $"Option with ID {optionId} not found." });

            try
            {
                // Price the option
                var pricingResult = await _pricingService.PriceOptionAsync(option, simParams);

                // Return combined response
                var response = new PricingResponseDTO
                {
                    Option = option,
                    PricingResult = pricingResult,
                    SimulationParameters = simParams
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                // Validation errors or invalid parameters
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Unexpected errors (numerical errors, overflow, etc.)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred during pricing.", details = ex.Message });
            }
        }
    }
}
