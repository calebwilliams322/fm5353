using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public class AsianOption : OptionBase
    {
        public AveragingType AveragingType { get; set; }

        public AsianOption(double initialPrice, double strike, DateTime expiry, bool isCall, AveragingType averagingType)
            : base(initialPrice, strike, expiry, isCall)
        {
            AveragingType = averagingType;
        }

        protected override bool RequiresPaths() => true;

        protected override List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p)
        {
            if (paths == null)
                throw new InvalidOperationException("Asian options require full paths.");

            Func<double[], double> payoff = path =>
            {
                double averagePrice = AveragingType == AveragingType.Arithmetic
                    ? path.Average()
                    : Math.Exp(path.Average(Math.Log)); // Geometric mean

                return IsCall
                    ? Math.Max(averagePrice - Strike, 0)
                    : Math.Max(Strike - averagePrice, 0);
            };

            return paths.Select(payoff).ToList();
        }
    }
}