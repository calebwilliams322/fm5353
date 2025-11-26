using MonteCarloAPI.Data;

namespace MonteCarloAPI.Models
{
    /// <summary>
    /// DTO for Position information
    /// </summary>
    public class PositionDTO
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public AssetType AssetType { get; set; }
        public int? StockId { get; set; }
        public int? OptionId { get; set; }
        public int NetQuantity { get; set; }
        public double AverageCost { get; set; }
        public double TotalCost { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }

        // Include asset details for convenience
        public StockDTO? Stock { get; set; }
        public OptionConfigDTO? Option { get; set; }

        /// <summary>
        /// Converts a PositionEntity to DTO
        /// </summary>
        public static PositionDTO FromEntity(PositionEntity entity)
        {
            return new PositionDTO
            {
                Id = entity.Id,
                PortfolioId = entity.PortfolioId,
                AssetType = entity.AssetType,
                StockId = entity.StockId,
                OptionId = entity.OptionId,
                NetQuantity = entity.NetQuantity,
                AverageCost = entity.AverageCost,
                TotalCost = entity.TotalCost,
                LastUpdated = entity.LastUpdated,
                CreatedAt = entity.CreatedAt
            };
        }
    }

    /// <summary>
    /// DTO for position with valuation information
    /// </summary>
    public class PositionWithValuationDTO
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public AssetType AssetType { get; set; }
        public int? StockId { get; set; }
        public int? OptionId { get; set; }
        public int NetQuantity { get; set; }
        public double AverageCost { get; set; }
        public double TotalCost { get; set; }
        public DateTime LastUpdated { get; set; }

        // Asset details (one will be populated based on AssetType)
        public StockDTO? Stock { get; set; }
        public OptionConfigDTO? Option { get; set; }

        // Valuation (computed from pricing engine or market price)
        public double CurrentPrice { get; set; }  // Price per share/contract
        public double CurrentValue { get; set; }  // NetQuantity * CurrentPrice
        public double UnrealizedPnL { get; set; } // CurrentValue - TotalCost
        public double PnLPercentage { get; set; } // (UnrealizedPnL / TotalCost) * 100

        // Greeks (only applicable for options, null for stocks)
        public double? Delta { get; set; }
        public double? Gamma { get; set; }
        public double? Vega { get; set; }
        public double? Theta { get; set; }
        public double? Rho { get; set; }
    }

    /// <summary>
    /// DTO for portfolio valuation request (market parameters)
    /// Note: InitialPrice comes from each option's underlying stock
    /// RiskFreeRate is looked up from the rate curve based on time to expiry
    /// TimeToExpiry is calculated from each option's ExpiryDate
    /// </summary>
    public class PortfolioValuationRequestDTO
    {
        public double Volatility { get; set; } = 0.2;
        public int TimeSteps { get; set; } = 252;
        public int NumberOfPaths { get; set; } = 10000;
        public bool UseMultithreading { get; set; } = true;
        public int SimMode { get; set; } = 0;
    }

    /// <summary>
    /// DTO for complete portfolio valuation response
    /// </summary>
    public class PortfolioValuationDTO
    {
        public int PortfolioId { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public double Cash { get; set; }

        // Positions with valuations (includes both stock and option positions)
        public List<PositionWithValuationDTO> Positions { get; set; } = new();

        // Separated totals for clarity
        public double StockPositionValue { get; set; }   // Sum of stock position values
        public double OptionPositionValue { get; set; }  // Sum of option position values
        public double TotalPositionValue { get; set; }   // Stock + Option position values
        public double TotalValue { get; set; }           // TotalPositionValue + Cash
        public double TotalCost { get; set; }            // Sum of all position costs
        public double TotalUnrealizedPnL { get; set; }   // Sum of all position P&L
        public double TotalPnLPercentage { get; set; }   // (TotalUnrealizedPnL / TotalCost) * 100

        // Portfolio Greeks (sum of all option position Greeks - stocks don't have Greeks)
        public double? PortfolioDelta { get; set; }
        public double? PortfolioGamma { get; set; }
        public double? PortfolioVega { get; set; }
        public double? PortfolioTheta { get; set; }
        public double? PortfolioRho { get; set; }

        // Market parameters used for valuation
        public SimulationParametersDTO MarketParameters { get; set; } = new();
    }
}
