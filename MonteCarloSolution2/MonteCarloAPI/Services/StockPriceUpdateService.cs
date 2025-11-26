using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MonteCarloAPI.Configuration;
using MonteCarloAPI.Data;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Background service that periodically updates stock prices from Alpaca API
    /// </summary>
    public class StockPriceUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockPriceUpdateService> _logger;
        private readonly AlpacaConfiguration _config;

        public StockPriceUpdateService(
            IServiceProvider serviceProvider,
            ILogger<StockPriceUpdateService> logger,
            IOptions<AlpacaConfiguration> config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock Price Update Service started");

            // Wait a bit before starting the first update
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateAllStockPricesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating stock prices");
                }

                // Wait for the configured interval before next update
                var interval = TimeSpan.FromMinutes(_config.UpdateIntervalMinutes);
                _logger.LogInformation("Next price update in {Minutes} minutes", _config.UpdateIntervalMinutes);
                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task UpdateAllStockPricesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var alpacaService = scope.ServiceProvider.GetRequiredService<AlpacaService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<MonteCarloDbContext>();

            _logger.LogInformation("Starting stock price update");

            // Get all stock tickers from database
            var stocks = await dbContext.Stocks.ToListAsync();
            if (stocks.Count == 0)
            {
                _logger.LogWarning("No stocks found in database");
                return;
            }

            var tickers = stocks.Select(s => s.Ticker).ToList();
            _logger.LogInformation("Fetching prices for {Count} stocks", tickers.Count);

            // Fetch latest prices from Alpaca
            var prices = await alpacaService.GetLatestPricesAsync(tickers);

            if (prices.Count == 0)
            {
                _logger.LogWarning("No prices returned from Alpaca API");
                return;
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
                else
                {
                    _logger.LogWarning("No price data for {Ticker}", stock.Ticker);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Updated prices for {Count}/{Total} stocks", updatedCount, stocks.Count);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stock Price Update Service stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}
