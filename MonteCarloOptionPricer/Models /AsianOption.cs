using System;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer.Models
{
    public class AsianOption : IOption
    {
        public Stock Stock { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // Additional field for Asian options
        public AveragingType AverageType { get; set; }

        public AsianOption(Stock stock, double strike, DateTime expiry, bool isCall, AveragingType averageType)
        {
            Stock = stock;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            AverageType = averageType;
        }

        public PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks)
        {
            throw new NotImplementedException();
        }
    }
}