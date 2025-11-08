using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public class RangeOption : OptionBase
    {
        public RangeOption(double initialPrice, double strike, DateTime expiry)
            : base(initialPrice, strike, expiry, isCall: true) // Strike is now included
        {
        }

        protected override bool RequiresPaths() => true; // Range options require full paths

        protected override List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p)
        {
            if (paths == null)
                throw new InvalidOperationException("Range options require full paths.");

            // Handle antithetic variates
            if (p.SimMode == SimulationMode.Antithetic)
            {
                var payoffs = new List<double>(paths.Count / 2);
                for (int i = 0; i < paths.Count / 2; i++)
                {
                    // Original path
                    double rangeA = paths[2 * i].Max() - paths[2 * i].Min();

                    // Antithetic path
                    double rangeB = paths[2 * i + 1].Max() - paths[2 * i + 1].Min();

                    // Average the ranges of the original and antithetic paths
                    payoffs.Add(0.5 * (rangeA + rangeB));
                }
                return payoffs;
            }

            // Standard payoff calculation
            return paths.Select(path => path.Max() - path.Min()).ToList();
        }
    }
}
