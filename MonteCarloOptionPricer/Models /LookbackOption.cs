using System;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.Simulation;


namespace MonteCarloOptionPricer.Models
{
    public class LookbackOption : IOption
    {
        public Stock Stock { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // Additional field for Lookback options
        public LookbackType LookbackOptionType { get; set; }

        public LookbackOption(Stock stock, double strike, DateTime expiry, bool isCall, LookbackType lookbackOptionType)
        {
            Stock = stock;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            LookbackOptionType = lookbackOptionType;
        }

        public PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks)
        {
            throw new NotImplementedException();
        }
    }
}