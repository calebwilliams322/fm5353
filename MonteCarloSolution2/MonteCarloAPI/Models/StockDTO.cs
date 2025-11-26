using MonteCarloAPI.Data;

namespace MonteCarloAPI.Models
{
    /// <summary>
    /// DTO for Stock information
    /// </summary>
    public class StockDTO
    {
        public int Id { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double CurrentPrice { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Exchange information
        public int ExchangeId { get; set; }
        public string ExchangeName { get; set; } = string.Empty;
        public string? ExchangeCountry { get; set; }
        public string? ExchangeCurrency { get; set; }

        /// <summary>
        /// Converts a StockEntity to DTO
        /// </summary>
        public static StockDTO FromEntity(StockEntity entity)
        {
            return new StockDTO
            {
                Id = entity.Id,
                Ticker = entity.Ticker,
                Name = entity.Name,
                CurrentPrice = entity.CurrentPrice,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ExchangeId = entity.ExchangeId,
                ExchangeName = entity.Exchange?.Name ?? "Unknown",
                ExchangeCountry = entity.Exchange?.Country,
                ExchangeCurrency = entity.Exchange?.Currency
            };
        }
    }

    /// <summary>
    /// DTO for creating a new stock
    /// </summary>
    public class CreateStockDTO
    {
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double CurrentPrice { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// Optional: Exchange ID. If not provided, will use default NYSE exchange.
        /// </summary>
        public int? ExchangeId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing stock
    /// </summary>
    public class UpdateStockDTO
    {
        public string? Ticker { get; set; }
        public string? Name { get; set; }
        public double? CurrentPrice { get; set; }
        public string? Description { get; set; }
    }
}
