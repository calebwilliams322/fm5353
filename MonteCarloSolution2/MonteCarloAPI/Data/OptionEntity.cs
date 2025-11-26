using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonteCarloAPI.Models;

namespace MonteCarloAPI.Data
{
    /// <summary>
    /// Database entity representing a stored option configuration.
    /// Maps to the "Options" table in PostgreSQL.
    /// </summary>
    [Table("Options")]
    public class OptionEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // --- Stock Reference (Foreign Key) ---
        /// <summary>
        /// Foreign key to the underlying stock
        /// </summary>
        [Required]
        public int StockId { get; set; }

        /// <summary>
        /// Navigation property to the underlying stock
        /// </summary>
        [ForeignKey(nameof(StockId))]
        public virtual StockEntity? Stock { get; set; }

        // --- Core parameters ---
        [Required]
        public OptionType OptionType { get; set; }

        [Required]
        public double Strike { get; set; }

        [Required]
        public bool IsCall { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        // --- Asian option parameters (NULL for non-Asian options) ---
        public AveragingType? AveragingType { get; set; }
        public int? ObservationFrequency { get; set; }

        // --- Digital option parameters (NULL for non-Digital options) ---
        public ConditionType? DigitalCondition { get; set; }

        // --- Barrier option parameters (NULL for non-Barrier options) ---
        public BarrierType? BarrierOptionType { get; set; }
        public BarrierDirection? BarrierDir { get; set; }
        public double? BarrierLevel { get; set; }

        // --- Lookback option parameters (NULL for non-Lookback options) ---
        public LookbackType? LookbackOptionType { get; set; }

        // --- Range option parameters (NULL for non-Range options) ---
        public int? RangeObservationFrequency { get; set; }

        // --- Metadata ---
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
