using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Data;
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
        private readonly MonteCarloDbContext _context;

        public PricingController(OptionService optionService, PricingService pricingService, MonteCarloDbContext context)
        {
            _optionService = optionService;
            _pricingService = pricingService;
            _context = context;
        }

        // GET: /api/pricing/all
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PriceAllOptions([FromQuery] double volatility = 0.2,
                                                          [FromQuery] int timeSteps = 252,
                                                          [FromQuery] int numberOfPaths = 10000,
                                                          [FromQuery] bool useMultithreading = true,
                                                          [FromQuery] int simMode = 0)
        {
            try
            {
                // Create simulation parameters from query string
                // Note: RiskFreeRate and TimeToExpiry are calculated automatically per option
                // based on the option's ExpiryDate and the rate curve
                var baseSimParams = new SimulationParametersDTO
                {
                    Volatility = volatility,
                    TimeSteps = timeSteps,
                    NumberOfPaths = numberOfPaths,
                    UseMultithreading = useMultithreading,
                    SimMode = (SimulationMode)simMode
                };

                // Get all options from database
                var allOptions = await _optionService.GetAllOptionsAsync();

                if (allOptions.Count == 0)
                {
                    return Ok(new { message = "No options found in database.", results = new List<PricingResponseDTO>() });
                }

                // Price each option
                var results = new List<PricingResponseDTO>();
                foreach (var option in allOptions)
                {
                    try
                    {
                        // Create per-option simulation parameters using the option's underlying stock price
                        // RiskFreeRate and TimeToExpiry will be calculated by PricingService based on ExpiryDate
                        var simParams = new SimulationParametersDTO
                        {
                            InitialPrice = option.Stock?.CurrentPrice ?? 100.0,
                            Volatility = baseSimParams.Volatility,
                            TimeSteps = baseSimParams.TimeSteps,
                            NumberOfPaths = baseSimParams.NumberOfPaths,
                            UseMultithreading = baseSimParams.UseMultithreading,
                            SimMode = baseSimParams.SimMode
                        };

                        var pricingResult = await _pricingService.PriceOptionAsync(option, simParams);

                        // Save to pricing history (simParams now has calculated RiskFreeRate and TimeToExpiry)
                        var historyEntry = new PricingHistoryEntity
                        {
                            OptionId = option.Id,
                            InitialPrice = simParams.InitialPrice,
                            Volatility = simParams.Volatility,
                            RiskFreeRate = simParams.RiskFreeRate,
                            TimeToExpiry = simParams.TimeToExpiry,
                            TimeSteps = simParams.TimeSteps,
                            NumberOfPaths = simParams.NumberOfPaths,
                            UseMultithreading = simParams.UseMultithreading,
                            SimMode = (int)simParams.SimMode,
                            Price = pricingResult.Price,
                            StandardError = pricingResult.StandardError ?? 0.0,
                            ExecutionTimeMs = 0.0,
                            Delta = pricingResult.Delta,
                            Gamma = pricingResult.Gamma,
                            Vega = pricingResult.Vega,
                            Theta = pricingResult.Theta,
                            Rho = pricingResult.Rho,
                            RequestSource = "API-PriceAll"
                        };
                        _context.PricingHistory.Add(historyEntry);
                        await _context.SaveChangesAsync();

                        results.Add(new PricingResponseDTO
                        {
                            Option = option,
                            PricingResult = pricingResult,
                            SimulationParameters = simParams
                        });
                    }
                    catch (Exception ex)
                    {
                        // If one option fails, continue with others but log the error
                        results.Add(new PricingResponseDTO
                        {
                            Option = option,
                            PricingResult = null,
                            SimulationParameters = baseSimParams,
                            ErrorMessage = $"Failed to price option {option.Id}: {ex.Message}"
                        });
                    }
                }

                return Ok(new
                {
                    count = results.Count,
                    baseParameters = new { volatility, timeSteps, numberOfPaths, useMultithreading, simMode },
                    results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while pricing options.", details = ex.Message });
            }
        }

        // POST: /api/pricing/{optionId}
        [HttpPost("{optionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PriceOption(int optionId, [FromBody] SimulationParametersDTO inputParams)
        {
            if (inputParams == null)
                return BadRequest(new { message = "SimulationParameters are required." });

            // Get the option configuration
            var option = await _optionService.GetOptionByIdAsync(optionId);
            if (option == null)
                return NotFound(new { message = $"Option with ID {optionId} not found." });

            try
            {
                // Create simulation parameters using the option's underlying stock price
                // RiskFreeRate and TimeToExpiry will be calculated by PricingService based on ExpiryDate
                var simParams = new SimulationParametersDTO
                {
                    InitialPrice = option.Stock?.CurrentPrice ?? 100.0,
                    Volatility = inputParams.Volatility,
                    TimeSteps = inputParams.TimeSteps,
                    NumberOfPaths = inputParams.NumberOfPaths,
                    UseMultithreading = inputParams.UseMultithreading,
                    SimMode = inputParams.SimMode
                };

                // Price the option (this calculates RiskFreeRate and TimeToExpiry)
                var pricingResult = await _pricingService.PriceOptionAsync(option, simParams);

                // Save to pricing history (simParams now has calculated RiskFreeRate and TimeToExpiry)
                var historyEntry = new PricingHistoryEntity
                {
                    OptionId = option.Id,
                    InitialPrice = simParams.InitialPrice,
                    Volatility = simParams.Volatility,
                    RiskFreeRate = simParams.RiskFreeRate,
                    TimeToExpiry = simParams.TimeToExpiry,
                    TimeSteps = simParams.TimeSteps,
                    NumberOfPaths = simParams.NumberOfPaths,
                    UseMultithreading = simParams.UseMultithreading,
                    SimMode = (int)simParams.SimMode,
                    Price = pricingResult.Price,
                    StandardError = pricingResult.StandardError ?? 0.0,
                    ExecutionTimeMs = 0.0,
                    Delta = pricingResult.Delta,
                    Gamma = pricingResult.Gamma,
                    Vega = pricingResult.Vega,
                    Theta = pricingResult.Theta,
                    Rho = pricingResult.Rho,
                    RequestSource = "API-Single"
                };
                _context.PricingHistory.Add(historyEntry);
                await _context.SaveChangesAsync();

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

        // GET: /api/pricing/history
        [HttpGet("history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPricingHistory([FromQuery] int? limit = 100)
        {
            try
            {
                var history = await _context.PricingHistory
                    .Include(h => h.Option)
                    .ThenInclude(o => o.Stock)
                    .OrderByDescending(h => h.PricedAt)
                    .Take(limit ?? 100)
                    .ToListAsync();

                var result = history.Select(h => new
                {
                    id = h.Id,
                    timestamp = h.PricedAt,
                    optionId = h.OptionId,
                    optionType = h.Option?.OptionType.ToString(),
                    stockSymbol = h.Option?.Stock?.Ticker,
                    stockPrice = h.InitialPrice,
                    strike = h.Option?.Strike,
                    price = h.Price,
                    volatility = h.Volatility,
                    riskFreeRate = h.RiskFreeRate,
                    timeToExpiry = h.TimeToExpiry,
                    timeSteps = h.TimeSteps,
                    numberOfPaths = h.NumberOfPaths,
                    simMode = h.SimMode,
                    requestSource = h.RequestSource,
                    delta = h.Delta,
                    gamma = h.Gamma,
                    vega = h.Vega,
                    theta = h.Theta,
                    rho = h.Rho
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred fetching pricing history.", details = ex.Message });
            }
        }
    }
}
