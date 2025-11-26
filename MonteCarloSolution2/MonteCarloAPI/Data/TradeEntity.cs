using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Database entity representing a trade - a transaction to buy or sell a stock or option.
    /// Maps to the "Trades" table in PostgreSQL.
    /// </summary>
    [Table("Trades")]
    public class TradeEntity
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
        /// Type of asset being traded (Stock or Option)
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
        /// Type of trade: Buy, Sell, or Close
        /// </summary>
        [Required]
        public TradeType TradeType { get; set; }

        /// <summary>
        /// Number of contracts traded (positive for buy, negative for sell)
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per contract at execution
        /// </summary>
        [Required]
        public double Price { get; set; }

        /// <summary>
        /// Total cost of the trade (Quantity * Price), includes sign
        /// </summary>
        [Required]
        public double TotalCost { get; set; }

        /// <summary>
        /// Optional notes about the trade
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Date when the trade was executed
        /// </summary>
        [Required]
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// Date when the trade was recorded in the system
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        // --- Navigation properties ---
        /// <summary>
        /// The portfolio this trade belongs to
        /// </summary>
        public PortfolioEntity Portfolio { get; set; } = null!;

        /// <summary>
        /// The stock being traded (if AssetType is Stock)
        /// </summary>
        public StockEntity? Stock { get; set; }

        /// <summary>
        /// The option contract being traded (if AssetType is Option)
        /// </summary>
        public OptionEntity? Option { get; set; }
    }

    /// <summary>
    /// Enum representing the type of trade
    /// </summary>
    public enum TradeType
    {
        Buy = 0,    // Opening or adding to a long position
        Sell = 1,   // Opening a short position or reducing a long position
        Close = 2   // Explicitly closing a position
    }

    /// <summary>
    /// Enum representing the type of asset being traded
    /// </summary>
    public enum AssetType
    {
        Stock = 0,  // Trading underlying stock shares
        Option = 1  // Trading option contracts
    }
}
