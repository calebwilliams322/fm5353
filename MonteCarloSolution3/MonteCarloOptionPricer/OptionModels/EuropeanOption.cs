using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public class EuropeanOption : OptionBase
    {
        public EuropeanOption(double initialPrice, double strike, DateTime expiry, bool isCall)
            : base(initialPrice, strike, expiry, isCall) { }

        protected override List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p)
        {
            Func<double, double> payoff = ST => IsCall
                ? Math.Max(ST - Strike, 0)
                : Math.Max(Strike - ST, 0);

            // Handle antithetic variates
            if (p.SimMode == SimulationMode.Antithetic)
            {
                var payoffs = new List<double>(terminals.Count / 2);
                for (int i = 0; i < terminals.Count / 2; i++)
                {
                    double a = payoff(terminals[2 * i]);       // First path
                    double b = payoff(terminals[2 * i + 1]);   // Antithetic path
                    payoffs.Add(0.5 * (a + b));               // Average the two payoffs
                }
                return payoffs;
            }

            // Standard payoff calculation
            return terminals.Select(payoff).ToList();
        }
    }

}