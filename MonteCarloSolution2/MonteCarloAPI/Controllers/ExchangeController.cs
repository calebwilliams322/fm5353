using Microsoft.AspNetCore.Mvc;
using MonteCarloAPI.Models;
using MonteCarloAPI.Services;

namespace MonteCarloAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeController : ControllerBase
    {
        private readonly ExchangeService _exchangeService;
        private readonly ILogger<ExchangeController> _logger;

        public ExchangeController(ExchangeService exchangeService, ILogger<ExchangeController> logger)
        {
            _exchangeService = exchangeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all exchanges
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllExchanges()
        {
            var exchanges = await _exchangeService.GetAllExchangesAsync();
            return Ok(exchanges);
        }

        /// <summary>
        /// Get exchange by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExchange(int id)
        {
            var exchange = await _exchangeService.GetExchangeByIdAsync(id);
            if (exchange == null)
                return NotFound(new { message = $"Exchange with ID {id} not found." });

            return Ok(exchange);
        }

        /// <summary>
        /// Create a new exchange
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateExchange([FromBody] CreateExchangeDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Exchange name is required." });

            try
            {
                var exchange = await _exchangeService.CreateExchangeAsync(dto);
                _logger.LogInformation("Created exchange: {Name}", exchange.Name);
                return CreatedAtAction(nameof(GetExchange), new { id = exchange.Id }, exchange);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete an exchange by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteExchange(int id)
        {
            try
            {
                var result = await _exchangeService.DeleteExchangeAsync(id);
                if (!result)
                    return NotFound(new { message = $"Exchange with ID {id} not found." });

                _logger.LogInformation("Deleted exchange with ID: {Id}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
