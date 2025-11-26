using Microsoft.EntityFrameworkCore;
using MonteCarloAPI.Data;
using MonteCarloAPI.Models;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Service for managing stocks/underlying assets.
    /// Handles business logic for stock CRUD operations.
    /// </summary>
    public class StockService
    {
        private readonly MonteCarloDbContext _context;
        private readonly ExchangeService _exchangeService;

        public StockService(MonteCarloDbContext context, ExchangeService exchangeService)
        {
            _context = context;
            _exchangeService = exchangeService;
        }

        /// <summary>
        /// Create a new stock
        /// </summary>
        public async Task<StockDTO> CreateStockAsync(CreateStockDTO createDto)
        {
            // Use provided ExchangeId or get default NYSE exchange
            int exchangeId = createDto.ExchangeId ?? await _exchangeService.EnsureDefaultExchangeAsync();

            // Check if ticker already exists on this exchange
            var existingStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.Ticker == createDto.Ticker.ToUpper() && s.ExchangeId == exchangeId);

            if (existingStock != null)
            {
                var exchange = await _context.Exchanges.FindAsync(exchangeId);
                throw new ArgumentException($"Stock with ticker '{createDto.Ticker}' already exists on {exchange?.Name ?? "this exchange"}");
            }

            var entity = new StockEntity
            {
                Ticker = createDto.Ticker.ToUpper(), // Store tickers in uppercase
                Name = createDto.Name,
                CurrentPrice = createDto.CurrentPrice,
                Description = createDto.Description,
                ExchangeId = exchangeId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stocks.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with Exchange included
            await _context.Entry(entity).Reference(e => e.Exchange).LoadAsync();

            return StockDTO.FromEntity(entity);
        }

        /// <summary>
        /// Get stock by ID
        /// </summary>
        public async Task<StockDTO?> GetStockByIdAsync(int id)
        {
            var entity = await _context.Stocks
                .Include(s => s.Exchange)
                .FirstOrDefaultAsync(s => s.Id == id);
            return entity == null ? null : StockDTO.FromEntity(entity);
        }

        /// <summary>
        /// Get stock by ticker symbol
        /// </summary>
        public async Task<StockDTO?> GetStockByTickerAsync(string ticker)
        {
            var entity = await _context.Stocks
                .Include(s => s.Exchange)
                .FirstOrDefaultAsync(s => s.Ticker == ticker.ToUpper());
            return entity == null ? null : StockDTO.FromEntity(entity);
        }

        /// <summary>
        /// Get all stocks
        /// </summary>
        public async Task<List<StockDTO>> GetAllStocksAsync()
        {
            var entities = await _context.Stocks
                .Include(s => s.Exchange)
                .OrderBy(s => s.Ticker)
                .ToListAsync();

            return entities.Select(StockDTO.FromEntity).ToList();
        }

        /// <summary>
        /// Update stock information
        /// </summary>
        public async Task<StockDTO?> UpdateStockAsync(int id, UpdateStockDTO updateDto)
        {
            var entity = await _context.Stocks
                .Include(s => s.Exchange)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null) return null;

            if (updateDto.Ticker != null)
            {
                // Check if new ticker conflicts with another stock on the same exchange
                var existingStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.Ticker == updateDto.Ticker.ToUpper() && s.ExchangeId == entity.ExchangeId && s.Id != id);

                if (existingStock != null)
                {
                    throw new ArgumentException($"Stock with ticker '{updateDto.Ticker}' already exists on {entity.Exchange?.Name ?? "this exchange"}");
                }

                entity.Ticker = updateDto.Ticker.ToUpper();
            }

            if (updateDto.Name != null)
                entity.Name = updateDto.Name;

            if (updateDto.CurrentPrice.HasValue)
                entity.CurrentPrice = updateDto.CurrentPrice.Value;

            if (updateDto.Description != null)
                entity.Description = updateDto.Description;

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return StockDTO.FromEntity(entity);
        }

        /// <summary>
        /// Delete stock (only if no options reference it)
        /// </summary>
        public async Task<bool> DeleteStockAsync(int id)
        {
            var entity = await _context.Stocks.FindAsync(id);
            if (entity == null) return false;

            // Check if any options reference this stock
            var optionsCount = await _context.Options
                .Where(o => o.StockId == id)
                .CountAsync();

            if (optionsCount > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete stock. {optionsCount} option(s) reference this stock.");
            }

            _context.Stocks.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
