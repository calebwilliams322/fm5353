using Alpaca.Markets;
using Microsoft.Extensions.Options;
using MonteCarloAPI.Configuration;

namespace MonteCarloAPI.Services
{
    /// <summary>
    /// Service for interacting with Alpaca API to fetch real-time stock prices
    /// </summary>
    public class AlpacaService
    {
        private readonly IAlpacaDataClient _dataClient;
        private readonly ILogger<AlpacaService> _logger;
        private readonly AlpacaConfiguration _config;

        public AlpacaService(IOptions<AlpacaConfiguration> config, ILogger<AlpacaService> logger)
        {
            _config = config.Value;
            _logger = logger;

            // Create Alpaca client
            var secretKey = new SecretKey(_config.ApiKey, _config.SecretKey);
            _dataClient = Alpaca.Markets.Environments.Paper.GetAlpacaDataClient(secretKey);
        }

        /// <summary>
        /// Fetch the latest price for a single stock ticker
        /// </summary>
        public async Task<decimal?> GetLatestPriceAsync(string ticker)
        {
            try
            {
                _logger.LogInformation("Fetching latest price for {Ticker}", ticker);

                var latestTrade = await _dataClient.GetLatestTradeAsync(
                    new LatestMarketDataRequest(ticker));

                if (latestTrade != null)
                {
                    _logger.LogInformation("Got price for {Ticker}: {Price}", ticker, latestTrade.Price);
                    return latestTrade.Price;
                }

                _logger.LogWarning("No trade data available for {Ticker}", ticker);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price for {Ticker}", ticker);
                return null;
            }
        }

        /// <summary>
        /// Fetch the latest prices for multiple stock tickers
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetLatestPricesAsync(IEnumerable<string> tickers)
        {
            var prices = new Dictionary<string, decimal>();

            try
            {
                _logger.LogInformation("Fetching latest prices for {Count} tickers", tickers.Count());

                var tickersList = tickers.ToList();
                var latestTrades = await _dataClient.ListLatestTradesAsync(
                    new LatestMarketDataListRequest(tickersList));

                foreach (var trade in latestTrades)
                {
                    if (trade.Value != null)
                    {
                        prices[trade.Key] = trade.Value.Price;
                    }
                }

                _logger.LogInformation("Successfully fetched {Count} prices", prices.Count);
                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching batch prices");
                return prices;
            }
        }

    }
}
