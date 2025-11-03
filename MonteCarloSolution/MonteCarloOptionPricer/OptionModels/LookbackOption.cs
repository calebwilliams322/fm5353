using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public class LookbackOption : OptionBase
    {
        public LookbackType LookbackOptionType { get; set; } // Max or Min for fixed/floating strike

        public LookbackOption(
            double initialPrice,
            double strike,
            DateTime expiry,
            bool isCall,
            LookbackType lookbackType = LookbackType.Max)
            : base(initialPrice, strike, expiry, isCall)
        {
            LookbackOptionType = lookbackType;
        }

        protected override bool RequiresPaths() => true; // Lookback options require full paths

        protected override List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p)
        {
            if (paths == null)
                throw new InvalidOperationException("Lookback options require full paths.");

            // Handle antithetic variates
            if (p.SimMode == SimulationMode.Antithetic)
            {
                var payoffs = new List<double>(paths.Count / 2);
                for (int i = 0; i < paths.Count / 2; i++)
                {
                    // Original path
                    double payoffA = CalculateLookbackPayoff(paths[2 * i]);

                    // Antithetic path
                    double payoffB = CalculateLookbackPayoff(paths[2 * i + 1]);

                    // Average the payoffs of the original and antithetic paths
                    payoffs.Add(0.5 * (payoffA + payoffB));
                }
                return payoffs;
            }

            // Standard payoff calculation
            return paths.Select(CalculateLookbackPayoff).ToList();
        }

        private double CalculateLookbackPayoff(double[] path)
        {
            double ST = path[^1]; // Final price
            double Smax = path.Max();
            double Smin = path.Min();

            // Fixed-strike or floating-strike payoff logic
            return LookbackOptionType switch
            {
                LookbackType.Max => IsCall
                    ? Math.Max(Smax - Strike, 0) // Fixed-strike call
                    : Math.Max(Strike - Smin, 0), // Fixed-strike put
                LookbackType.Min => IsCall
                    ? Math.Max(ST - Smin, 0) // Floating-strike call
                    : Math.Max(Smax - ST, 0), // Floating-strike put
                _ => throw new InvalidOperationException("Invalid LookbackOptionType.")
            };
        }
    }
}