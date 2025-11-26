using Microsoft.AspNetCore.Mvc;
using MonteCarloAPI.Models;
using MonteCarloAPI.Services;

namespace MonteCarloAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioService _portfolioService;

        public PortfolioController(PortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        // GET: /api/portfolio
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PortfolioDTO>>> GetAllPortfolios()
        {
            var portfolios = await _portfolioService.GetAllPortfoliosAsync();
            return Ok(portfolios);
        }

        // GET: /api/portfolio/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PortfolioDTO>> GetPortfolio(int id)
        {
            var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
            if (portfolio == null)
                return NotFound(new { message = $"Portfolio {id} not found" });

            return Ok(portfolio);
        }

        // GET: /api/portfolio/{id}/summary
        [HttpGet("{id}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PortfolioSummaryDTO>> GetPortfolioSummary(int id)
        {
            var summary = await _portfolioService.GetPortfolioSummaryAsync(id);
            if (summary == null)
                return NotFound(new { message = $"Portfolio {id} not found" });

            return Ok(summary);
        }

        // POST: /api/portfolio
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PortfolioDTO>> CreatePortfolio([FromBody] CreatePortfolioDTO createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                return BadRequest(new { message = "Portfolio name is required" });

            var portfolio = await _portfolioService.CreatePortfolioAsync(createDto);
            return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
        }

        // PUT: /api/portfolio/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PortfolioDTO>> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDTO updateDto)
        {
            var portfolio = await _portfolioService.UpdatePortfolioAsync(id, updateDto);
            if (portfolio == null)
                return NotFound(new { message = $"Portfolio {id} not found" });

            return Ok(portfolio);
        }

        // DELETE: /api/portfolio/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePortfolio(int id)
        {
            var deleted = await _portfolioService.DeletePortfolioAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Portfolio {id} not found" });

            return NoContent();
        }

        // GET: /api/portfolio/{id}/positions
        [HttpGet("{id}/positions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PositionDTO>>> GetPortfolioPositions(int id)
        {
            var positions = await _portfolioService.GetPortfolioPositionsAsync(id);
            return Ok(positions);
        }

        // GET: /api/portfolio/{id}/trades
        [HttpGet("{id}/trades")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TradeDTO>>> GetPortfolioTrades(int id)
        {
            var trades = await _portfolioService.GetPortfolioTradesAsync(id);
            return Ok(trades);
        }

        // POST: /api/portfolio/{id}/trades
        [HttpPost("{id}/trades")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TradeDTO>> RecordTrade(int id, [FromBody] CreateTradeDTO createDto)
        {
            try
            {
                var trade = await _portfolioService.RecordTradeAsync(id, createDto);
                return CreatedAtAction(nameof(GetTrade), new { portfolioId = id, tradeId = trade.Id }, trade);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /api/portfolio/{portfolioId}/trades/{tradeId}
        [HttpGet("{portfolioId}/trades/{tradeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TradeDTO>> GetTrade(int portfolioId, int tradeId)
        {
            var trade = await _portfolioService.GetTradeByIdAsync(tradeId);
            if (trade == null || trade.PortfolioId != portfolioId)
                return NotFound(new { message = $"Trade {tradeId} not found in portfolio {portfolioId}" });

            return Ok(trade);
        }

        // POST: /api/portfolio/{id}/value
        [HttpPost("{id}/value")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PortfolioValuationDTO>> ValuePortfolio(int id, [FromBody] PortfolioValuationRequestDTO request)
        {
            try
            {
                var valuation = await _portfolioService.ValuePortfolioAsync(id, request);
                return Ok(valuation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: /api/portfolio/{portfolioId}/positions/{positionId}
        [HttpDelete("{portfolioId}/positions/{positionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePosition(int portfolioId, int positionId)
        {
            var deleted = await _portfolioService.DeletePositionAsync(positionId);
            if (!deleted)
                return NotFound(new { message = $"Position {positionId} not found" });

            return NoContent();
        }

        // DELETE: /api/portfolio/{portfolioId}/trades/{tradeId}
        [HttpDelete("{portfolioId}/trades/{tradeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTrade(int portfolioId, int tradeId)
        {
            var deleted = await _portfolioService.DeleteTradeAsync(tradeId);
            if (!deleted)
                return NotFound(new { message = $"Trade {tradeId} not found" });

            return NoContent();
        }
    }
}
