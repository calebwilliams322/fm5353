using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Database entity representing a portfolio - a collection of option positions.
    /// Maps to the "Portfolios" table in PostgreSQL.
    /// </summary>
    [Table("Portfolios")]
    public class PortfolioEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Available cash balance in the portfolio
        /// </summary>
        [Required]
        public double Cash { get; set; }

        // --- Metadata ---
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // --- Navigation properties ---
        /// <summary>
        /// All trades belonging to this portfolio
        /// </summary>
        public ICollection<TradeEntity> Trades { get; set; } = new List<TradeEntity>();

        /// <summary>
        /// All positions belonging to this portfolio
        /// </summary>
        public ICollection<PositionEntity> Positions { get; set; } = new List<PositionEntity>();
    }
}
