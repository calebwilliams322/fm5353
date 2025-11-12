using System;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    /// <summary>
    /// Base interface for all option types.
    /// Each option can compute its own price (and optionally Greeks)
    /// using its parameters and the Monte Carlo simulator.
    /// </summary>
    public interface IOption
    {
        // --- Core option properties ---
        double Strike { get; set; }
        double InitialPrice { get; set; }
        DateTime Expiry { get; set; }
        bool IsCall { get; set; }

        // --- Pricing entry point ---
        PricingResult GetPrice(
            double volatility,
            double riskFreeRate,
            int timeSteps,
            int numberOfPaths,
            bool calculateGreeks = false,
            bool useMultithreading = true,
            SimulationMode simMode = SimulationMode.Plain);
    }
}