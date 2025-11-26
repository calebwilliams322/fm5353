using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Data;
using MonteCarloAPI.Models;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Service for managing exchanges
    /// </summary>
    public class ExchangeService
    {
        private readonly MonteCarloDbContext _context;

        public ExchangeService(MonteCarloDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all exchanges
        /// </summary>
        public async Task<List<ExchangeDTO>> GetAllExchangesAsync()
        {
            var exchanges = await _context.Exchanges
                .OrderBy(e => e.Name)
                .ToListAsync();

            return exchanges.Select(ExchangeDTO.FromEntity).ToList();
        }

        /// <summary>
        /// Get exchange by ID
        /// </summary>
        public async Task<ExchangeDTO?> GetExchangeByIdAsync(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);
            return exchange == null ? null : ExchangeDTO.FromEntity(exchange);
        }

        /// <summary>
        /// Get exchange by name
        /// </summary>
        public async Task<ExchangeDTO?> GetExchangeByNameAsync(string name)
        {
            var exchange = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.Name == name);
            return exchange == null ? null : ExchangeDTO.FromEntity(exchange);
        }

        /// <summary>
        /// Create a new exchange and auto-seed with default stocks from NYSE
        /// </summary>
        public async Task<ExchangeDTO> CreateExchangeAsync(CreateExchangeDTO dto)
        {
            // Check if exchange with same name already exists
            var existing = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.Name == dto.Name);
            if (existing != null)
            {
                throw new ArgumentException($"Exchange with name '{dto.Name}' already exists.");
            }

            var entity = new ExchangeEntity
            {
                Name = dto.Name,
                Description = dto.Description,
                Country = dto.Country,
                Currency = dto.Currency,
                CreatedAt = DateTime.UtcNow
            };

            _context.Exchanges.Add(entity);
            await _context.SaveChangesAsync();

            // Auto-seed stocks from NYSE to the new exchange
            await SeedStocksFromNyseAsync(entity.Id);

            return ExchangeDTO.FromEntity(entity);
        }

        /// <summary>
        /// Copy all stocks from NYSE to a new exchange
        /// </summary>
        private async Task SeedStocksFromNyseAsync(int newExchangeId)
        {
            // Get the NYSE exchange
            var nyse = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.Name == "NYSE");

            if (nyse == null)
                return; // No NYSE exchange to copy from

            // Get all stocks from NYSE (excluding DEFAULT stock)
            var nyseStocks = await _context.Stocks
                .Where(s => s.ExchangeId == nyse.Id && s.Ticker != "DEFAULT")
                .ToListAsync();

            // Create copies for the new exchange
            foreach (var stock in nyseStocks)
            {
                var newStock = new StockEntity
                {
                    Ticker = stock.Ticker,
                    Name = stock.Name,
                    CurrentPrice = stock.CurrentPrice,
                    Description = stock.Description,
                    ExchangeId = newExchangeId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Stocks.Add(newStock);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete an exchange by ID
        /// </summary>
        public async Task<bool> DeleteExchangeAsync(int id)
        {
            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange == null)
                return false;

            // Check if any stocks are using this exchange
            var stockCount = await _context.Stocks.CountAsync(s => s.ExchangeId == id);
            if (stockCount > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete exchange '{exchange.Name}' because {stockCount} stock(s) are listed on it.");
            }

            _context.Exchanges.Remove(exchange);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Ensure the default NYSE exchange exists, creating it if necessary
        /// Returns the exchange ID
        /// </summary>
        public async Task<int> EnsureDefaultExchangeAsync()
        {
            var nyse = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.Name == "NYSE");

            if (nyse != null)
                return nyse.Id;

            // Create NYSE as the default exchange
            var exchange = new ExchangeEntity
            {
                Name = "NYSE",
                Description = "New York Stock Exchange",
                Country = "USA",
                Currency = "USD",
                CreatedAt = DateTime.UtcNow
            };

            _context.Exchanges.Add(exchange);
            await _context.SaveChangesAsync();

            return exchange.Id;
        }
    }
}
