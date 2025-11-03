using System;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.Models
{
    public class RangeOption : IOption
    {
        public Stock Stock { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // Additional field for Range options
        public int RangeObservationFrequency { get; set; }

        public RangeOption(Stock stock, double strike, DateTime expiry, bool isCall, int rangeObservationFrequency)
        {
            Stock = stock;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            RangeObservationFrequency = rangeObservationFrequency;
        }

        public PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks)
        {
            throw new NotImplementedException();
        }
    }
}