using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    /// <summary>
    /// Represents a Digital (Binary) Option — either cash-or-nothing or asset-or-nothing.
    /// Pays a fixed amount if the underlying ends above (for a call) or below (for a put) the strike.
    /// </summary>
    public class DigitalOption : OptionBase
    {
        public bool IsCashOrNothing { get; set; } = true; // Determines if it's cash-or-nothing or asset-or-nothing
        public double Payout { get; set; } = 1.0;         // Fixed payout amount if condition is met

        public DigitalOption(
            double initialPrice,
            double strike,
            DateTime expiry,
            bool isCall,
            bool isCashOrNothing = true,
            double payout = 1.0)
            : base(initialPrice, strike, expiry, isCall)
        {
            IsCashOrNothing = isCashOrNothing;
            Payout = payout;
        }

        protected override bool RequiresPaths() => false; // Digital options only need terminal prices

        protected override List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p)
        {
            Func<double, double> payoff;

            if (IsCashOrNothing)
            {
                // Pays a fixed cash amount if the condition is met
                payoff = ST =>
                    IsCall
                    ? (ST > Strike ? Payout : 0.0)
                    : (ST < Strike ? Payout : 0.0);
            }
            else
            {
                // Pays the asset value if the condition is met
                payoff = ST =>
                    IsCall
                    ? (ST > Strike ? ST : 0.0)
                    : (ST < Strike ? ST : 0.0);
            }

            // Handle antithetic variates
            if (p.SimMode == SimulationMode.Antithetic)
            {
                var payoffs = new List<double>(terminals.Count / 2);
                for (int i = 0; i < terminals.Count / 2; i++)
                {
                    double a = payoff(terminals[2 * i]);       // Original terminal price
                    double b = payoff(terminals[2 * i + 1]);   // Antithetic terminal price
                    payoffs.Add(0.5 * (a + b));               // Average the two payoffs
                }
                return payoffs;
            }

            // Standard payoff calculation
            return terminals.Select(payoff).ToList();
        }
    }
}