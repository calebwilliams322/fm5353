using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Entity representing a stock/underlying asset in the database
    /// </summary>
    [Table("Stocks")]
    public class StockEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Stock ticker symbol (e.g., "AAPL", "MSFT")
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Ticker { get; set; } = string.Empty;

        /// <summary>
        /// Company or asset name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Current market price of the stock
        /// </summary>
        [Required]
        public double CurrentPrice { get; set; }

        /// <summary>
        /// Optional description or notes about the stock
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// When this stock was added to the system
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last time this stock was updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Foreign key to the exchange where this stock is listed
        /// </summary>
        [Required]
        public int ExchangeId { get; set; }

        /// <summary>
        /// Navigation property: The exchange where this stock is listed
        /// </summary>
        [ForeignKey("ExchangeId")]
        public virtual ExchangeEntity Exchange { get; set; } = null!;

        /// <summary>
        /// Navigation property: Options based on this stock
        /// </summary>
        public virtual ICollection<OptionEntity> Options { get; set; } = new List<OptionEntity>();
    }
}
