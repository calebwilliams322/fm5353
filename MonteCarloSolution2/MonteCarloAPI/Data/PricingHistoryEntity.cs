using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonteCarloAPI.Data
{
    [Table("PricingHistory")]
    public class PricingHistoryEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Foreign key to the Option that was priced
        [Required]
        public int OptionId { get; set; }

        // Navigation property
        [ForeignKey("OptionId")]
        public OptionEntity Option { get; set; } = null!;

        // Simulation parameters used for this pricing run
        [Required]
        public double InitialPrice { get; set; }

        [Required]
        public double Volatility { get; set; }

        [Required]
        public double RiskFreeRate { get; set; }

        [Required]
        public double TimeToExpiry { get; set; }

        [Required]
        public int TimeSteps { get; set; }

        [Required]
        public int NumberOfPaths { get; set; }

        [Required]
        public bool UseMultithreading { get; set; }

        [Required]
        public int SimMode { get; set; }

        // Pricing results
        [Required]
        public double Price { get; set; }

        [Required]
        public double StandardError { get; set; }

        [Required]
        public double ExecutionTimeMs { get; set; }

        // Greeks (optional - may be NULL if not calculated)
        public double? Delta { get; set; }
        public double? Gamma { get; set; }
        public double? Vega { get; set; }
        public double? Theta { get; set; }
        public double? Rho { get; set; }

        // Timestamp when this pricing was performed
        [Required]
        public DateTime PricedAt { get; set; }

        // Optional field to track who/what requested the pricing
        public string? RequestSource { get; set; }
    }
}
