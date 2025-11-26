using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Entity representing a stock exchange in the database
    /// </summary>
    [Table("Exchanges")]
    public class ExchangeEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Exchange name (e.g., "NYSE", "NASDAQ", "LSE")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the exchange
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Country where the exchange is located (e.g., "USA", "UK")
        /// </summary>
        [StringLength(100)]
        public string? Country { get; set; }

        /// <summary>
        /// Currency used on this exchange (e.g., "USD", "GBP", "EUR")
        /// </summary>
        [StringLength(10)]
        public string? Currency { get; set; }

        /// <summary>
        /// When this exchange was added to the system
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property: Stocks listed on this exchange
        /// </summary>
        public virtual ICollection<StockEntity> Stocks { get; set; } = new List<StockEntity>();
    }
}
