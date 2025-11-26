using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Data;
using MonteCarloAPI.Models;
using MonteCarloAPI.Services;

namespace MonteCarloAPI.Controllers
{
    /// <summary>
    /// API controller for managing stocks/underlying assets
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly StockService _stockService;
        private readonly ILogger<StockController> _logger;

        public StockController(StockService stockService, ILogger<StockController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/stock - Get all stocks
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<StockDTO>>> GetAllStocks()
        {
            try
            {
                var stocks = await _stockService.GetAllStocksAsync();
                return Ok(stocks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stocks");
                return StatusCode(500, new { message = "Error retrieving stocks", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/stock/{id} - Get stock by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<StockDTO>> GetStockById(int id)
        {
            try
            {
                var stock = await _stockService.GetStockByIdAsync(id);
                if (stock == null)
                {
                    return NotFound(new { message = $"Stock with ID {id} not found" });
                }
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock {StockId}", id);
                return StatusCode(500, new { message = "Error retrieving stock", error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/stock/ticker/{ticker} - Get stock by ticker symbol
        /// </summary>
        [HttpGet("ticker/{ticker}")]
        public async Task<ActionResult<StockDTO>> GetStockByTicker(string ticker)
        {
            try
            {
                var stock = await _stockService.GetStockByTickerAsync(ticker);
                if (stock == null)
                {
                    return NotFound(new { message = $"Stock with ticker '{ticker}' not found" });
                }
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock {Ticker}", ticker);
                return StatusCode(500, new { message = "Error retrieving stock", error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/stock/update-prices - Manually trigger stock price updates from Alpaca
        /// </summary>
        [HttpPost("update-prices")]
        public async Task<ActionResult> UpdatePrices([FromServices] AlpacaService alpacaService, [FromServices] MonteCarloDbContext dbContext)
        {
            try
            {
                _logger.LogInformation("Manual price update requested");

                // Get all stock tickers from database
                var stocks = await dbContext.Stocks.ToListAsync();
                if (stocks.Count == 0)
                {
                    return Ok(new { message = "No stocks found in database", updatedCount = 0 });
                }

                var tickers = stocks.Select(s => s.Ticker).ToList();
                _logger.LogInformation("Fetching prices for {Count} stocks", tickers.Count);

                // Fetch latest prices from Alpaca
                var prices = await alpacaService.GetLatestPricesAsync(tickers);

                if (prices.Count == 0)
                {
                    return Ok(new { message = "No prices returned from Alpaca API", updatedCount = 0 });
                }

                // Update stock prices in database
                int updatedCount = 0;
                foreach (var stock in stocks)
                {
                    if (prices.TryGetValue(stock.Ticker, out var newPrice))
                    {
                        stock.CurrentPrice = (double)newPrice;
                        stock.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated prices for {Count}/{Total} stocks", updatedCount, stocks.Count);

                return Ok(new { message = $"Updated prices for {updatedCount} stocks", updatedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock prices");
                return StatusCode(500, new { message = "Error updating stock prices", error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/stock/reset-data - Delete all portfolios, positions, trades, and options (keeps stocks)
        /// </summary>
        [HttpPost("reset-data")]
        public async Task<ActionResult> ResetData([FromServices] MonteCarloDbContext dbContext)
        {
            try
            {
                _logger.LogInformation("Data reset requested");

                // Delete in order: Trades -> Positions -> Portfolios, Options
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Trades\"");
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Positions\"");
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Portfolios\"");
                await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Options\"");

                _logger.LogInformation("All portfolios, positions, trades, and options deleted");

                return Ok(new { message = "All data reset successfully. Stocks preserved." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting data");
                return StatusCode(500, new { message = "Error resetting data", error = ex.Message });
            }
        }

        // Stock CRUD endpoints removed - stocks are managed by Alpaca integration only
        // Only read endpoints (GET) are available
    }
}
