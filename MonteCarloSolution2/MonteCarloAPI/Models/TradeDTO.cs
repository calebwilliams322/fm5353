using MonteCarloAPI.Data;

namespace MonteCarloAPI.Models
{
    /// <summary>
    /// DTO for Trade information
    /// </summary>
    public class TradeDTO
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public AssetType AssetType { get; set; }
        public int? StockId { get; set; }
        public int? OptionId { get; set; }
        public TradeType TradeType { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double TotalCost { get; set; }
        public string? Notes { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Include asset details for convenience
        public StockDTO? Stock { get; set; }
        public OptionConfigDTO? Option { get; set; }

        /// <summary>
        /// Converts a TradeEntity to DTO
        /// </summary>
        public static TradeDTO FromEntity(TradeEntity entity)
        {
            return new TradeDTO
            {
                Id = entity.Id,
                PortfolioId = entity.PortfolioId,
                AssetType = entity.AssetType,
                StockId = entity.StockId,
                OptionId = entity.OptionId,
                TradeType = entity.TradeType,
                Quantity = entity.Quantity,
                Price = entity.Price,
                TotalCost = entity.TotalCost,
                Notes = entity.Notes,
                TradeDate = entity.TradeDate,
                CreatedAt = entity.CreatedAt
            };
        }
    }

    /// <summary>
    /// DTO for creating a new trade
    /// </summary>
    public class CreateTradeDTO
    {
        public AssetType AssetType { get; set; }
        public int? StockId { get; set; }
        public int? OptionId { get; set; }
        public TradeType TradeType { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string? Notes { get; set; }
        public DateTime? TradeDate { get; set; } // Defaults to now if not provided
    }
}
