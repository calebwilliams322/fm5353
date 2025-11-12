using System;

namespace MonteCarloAPI.Models
{
    // DTO representing the output of a pricing simulation.
    // Matches your backend PricingResult class (names, nullability, and label).
    public class PricingResultDTO
    {
        public double Price { get; set; }                         // Estimated option price
        public double? StandardError { get; set; }                // Monte Carlo standard error

        // --- Greeks (nullable to handle cases where they aren't computed) ---
        public double? Delta { get; set; }
        public double? Gamma { get; set; }
        public double? Vega { get; set; }
        public double? Theta { get; set; }
        public double? Rho { get; set; }

        public string? Label { get; set; }                        // Optional tag or descriptor

        // --- API Metadata ---
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? OptionConfigId { get; set; }                  // ID reference for stored option
        public string? SimulationMode { get; set; }               // e.g., "Plain", "Antithetic", etc.
    }
}
