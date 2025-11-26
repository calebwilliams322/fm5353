using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Database entity representing a position - current holding in a stock or option.
    /// This is a materialized view of trades, kept in sync for performance.
    /// Maps to the "Positions" table in PostgreSQL.
    /// </summary>
    [Table("Positions")]
    public class PositionEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to Portfolio
        /// </summary>
        [Required]
        public int PortfolioId { get; set; }

        /// <summary>
        /// Type of asset in this position (Stock or Option)
        /// </summary>
        [Required]
        public AssetType AssetType { get; set; }

        /// <summary>
        /// Foreign key to Stock (required if AssetType is Stock)
        /// </summary>
        public int? StockId { get; set; }

        /// <summary>
        /// Foreign key to Option (required if AssetType is Option)
        /// </summary>
        public int? OptionId { get; set; }

        /// <summary>
        /// Current net quantity (sum of all trades for this option in this portfolio)
        /// Positive = long position, Negative = short position, Zero = no position
        /// </summary>
        [Required]
        public int NetQuantity { get; set; }

        /// <summary>
        /// Average cost per contract (cost basis)
        /// </summary>
        [Required]
        public double AverageCost { get; set; }

        /// <summary>
        /// Total cost invested in this position (including commissions)
        /// </summary>
        [Required]
        public double TotalCost { get; set; }

        /// <summary>
        /// Last time this position was updated (trade added/removed)
        /// </summary>
        [Required]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// When this position was first opened
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        // --- Navigation properties ---
        /// <summary>
        /// The portfolio this position belongs to
        /// </summary>
        public PortfolioEntity Portfolio { get; set; } = null!;

        /// <summary>
        /// The stock for this position (if AssetType is Stock)
        /// </summary>
        public StockEntity? Stock { get; set; }

        /// <summary>
        /// The option contract for this position (if AssetType is Option)
        /// </summary>
        public OptionEntity? Option { get; set; }
    }
}
