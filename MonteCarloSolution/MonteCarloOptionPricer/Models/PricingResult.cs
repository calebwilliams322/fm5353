using System;

namespace MonteCarloOptionPricer.Models
{
    /// <summary>
    /// Container for results returned by option pricing methods.
    /// Used across all option types and simulation modes.
    /// </summary>
    public class PricingResult
    {
        /// <summary>
        /// The estimated fair value (present value) of the option.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// The Monte Carlo standard error of the price estimate (if applicable).
        /// </summary>
        public double? StandardError { get; set; }

        /// <summary>
        /// Option sensitivity to underlying price.
        /// </summary>
        public double? Delta { get; set; }

        /// <summary>
        /// Option sensitivity to underlying price curvature.
        /// </summary>
        public double? Gamma { get; set; }

        /// <summary>
        /// Option sensitivity to volatility changes.
        /// </summary>
        public double? Vega { get; set; }

        /// <summary>
        /// Option sensitivity to time decay.
        /// </summary>
        public double? Theta { get; set; }

        /// <summary>
        /// Option sensitivity to interest rate changes.
        /// </summary>
        public double? Rho { get; set; }

        /// <summary>
        /// Optional free-form tag or identifier (e.g., option type, timestamp).
        /// </summary>
        public string? Label { get; set; }

        // --- Factory Methods ---

        /// <summary>
        /// Creates a basic result with only price and standard error.
        /// </summary>
        public static PricingResult From(double price, double? stderr = null)
        {
            return new PricingResult { Price = price, StandardError = stderr };
        }

        /// <summary>
        /// Creates a result with price and all Greeks (Delta, Gamma, Vega, Theta, Rho).
        /// </summary>
        public static PricingResult WithGreeks(double price, double? delta, double? gamma, double? vega, double? theta, double? rho)
        {
            return new PricingResult
            {
                Price = price,
                Delta = delta,
                Gamma = gamma,
                Vega = vega,
                Theta = theta,
                Rho = rho
            };
        }

        /// <summary>
        /// Creates a result with price, standard error, and a label.
        /// </summary>
        public static PricingResult WithLabel(double price, double? stderr, string label)
        {
            return new PricingResult
            {
                Price = price,
                StandardError = stderr,
                Label = label
            };
        }

        /// <summary>
        /// Creates a result with price, standard error, and Greeks.
        /// </summary>
        public static PricingResult WithGreeksAndError(double price, double? stderr, double? delta, double? gamma, double? vega, double? theta, double? rho)
        {
            return new PricingResult
            {
                Price = price,
                StandardError = stderr,
                Delta = delta,
                Gamma = gamma,
                Vega = vega,
                Theta = theta,
                Rho = rho
            };
        }
    }
}