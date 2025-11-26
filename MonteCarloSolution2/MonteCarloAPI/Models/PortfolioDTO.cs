using MonteCarloAPI.Data;

namespace MonteCarloAPI.Models
{
    /// <summary>
    /// DTO for Portfolio information (without positions/trades)
    /// </summary>
    public class PortfolioDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Cash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Converts a PortfolioEntity to DTO
        /// </summary>
        public static PortfolioDTO FromEntity(PortfolioEntity entity)
        {
            return new PortfolioDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Cash = entity.Cash,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }

    /// <summary>
    /// DTO for creating a new portfolio
    /// </summary>
    public class CreatePortfolioDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double InitialCash { get; set; } = 0.0;
    }

    /// <summary>
    /// DTO for updating an existing portfolio
    /// </summary>
    public class UpdatePortfolioDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? Cash { get; set; }
    }

    /// <summary>
    /// DTO for portfolio with summary statistics
    /// </summary>
    public class PortfolioSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Cash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Summary statistics
        public int PositionCount { get; set; }
        public int TradeCount { get; set; }
        public double TotalInvested { get; set; }
    }
}
