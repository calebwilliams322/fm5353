using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public class BarrierOption : OptionBase
    {
        public BarrierType BarrierOptionType { get; set; }    // KnockIn / KnockOut
        public BarrierDirection BarrierDir { get; set; }      // Up / Down
        public double BarrierLevel { get; set; }              // Barrier price level

        public BarrierOption(
            double initialPrice,
            double strike,
            DateTime expiry,
            bool isCall,
            BarrierType barrierType,
            BarrierDirection barrierDir,
            double barrierLevel)
            : base(initialPrice, strike, expiry, isCall)
        {
            BarrierOptionType = barrierType;
            BarrierDir = barrierDir;
            BarrierLevel = barrierLevel;
        }

        protected override bool RequiresPaths() => true;

        protected override List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p)
        {
            if (paths == null)
                throw new InvalidOperationException("Barrier options require full paths.");

            // Handle antithetic variates
            if (p.SimMode == SimulationMode.Antithetic)
            {
                var payoffs = new List<double>(paths.Count / 2);
                for (int i = 0; i < paths.Count / 2; i++)
                {
                    // Original path
                    bool barrierHitA = BarrierDir switch
                    {
                        BarrierDirection.Up => paths[2 * i].Any(S => S >= BarrierLevel),
                        BarrierDirection.Down => paths[2 * i].Any(S => S <= BarrierLevel),
                        _ => false
                    };

                    bool activeA = (BarrierOptionType == BarrierType.KnockIn && barrierHitA) ||
                                   (BarrierOptionType == BarrierType.KnockOut && !barrierHitA);

                    double payoffA = activeA
                        ? (IsCall ? Math.Max(paths[2 * i][^1] - Strike, 0) : Math.Max(Strike - paths[2 * i][^1], 0))
                        : 0.0;

                    // Antithetic path
                    bool barrierHitB = BarrierDir switch
                    {
                        BarrierDirection.Up => paths[2 * i + 1].Any(S => S >= BarrierLevel),
                        BarrierDirection.Down => paths[2 * i + 1].Any(S => S <= BarrierLevel),
                        _ => false
                    };

                    bool activeB = (BarrierOptionType == BarrierType.KnockIn && barrierHitB) ||
                                   (BarrierOptionType == BarrierType.KnockOut && !barrierHitB);

                    double payoffB = activeB
                        ? (IsCall ? Math.Max(paths[2 * i + 1][^1] - Strike, 0) : Math.Max(Strike - paths[2 * i + 1][^1], 0))
                        : 0.0;

                    // Average the payoffs of the original and antithetic paths
                    payoffs.Add(0.5 * (payoffA + payoffB));
                }
                return payoffs;
            }

            // Standard payoff calculation
            var standardPayoffs = new List<double>(paths.Count);
            foreach (var path in paths)
            {
                // Check if the barrier was hit
                bool barrierHit = BarrierDir switch
                {
                    BarrierDirection.Up => path.Any(S => S >= BarrierLevel),
                    BarrierDirection.Down => path.Any(S => S <= BarrierLevel),
                    _ => false
                };

                // Determine if the option is active based on the barrier type
                bool active = (BarrierOptionType == BarrierType.KnockIn && barrierHit) ||
                              (BarrierOptionType == BarrierType.KnockOut && !barrierHit);

                // Calculate the payoff based on the final price
                double payoff = active
                    ? (IsCall ? Math.Max(path[^1] - Strike, 0) : Math.Max(Strike - path[^1], 0))
                    : 0.0;

                standardPayoffs.Add(payoff);
            }

            return standardPayoffs;
        }
    }
}