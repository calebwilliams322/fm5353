using System;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer.Models
{
    public class DigitalOption : IOption
    {
        public Stock Stock { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // Additional field for Digital options
        public double PayoutAmount { get; set; }

        public DigitalOption(Stock stock, double strike, DateTime expiry, bool isCall, double payoutAmount)
        {
            Stock = stock;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            PayoutAmount = payoutAmount;
        }

        public PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks)
        {
            throw new NotImplementedException();
        }
    }
}