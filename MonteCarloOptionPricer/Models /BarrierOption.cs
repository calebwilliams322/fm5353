using System;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer.Models
{
    public class BarrierOption : IOption
    {
        public Stock Stock { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // Additional fields for Barrier options
        public BarrierType BarrierOptionType { get; set; }
        public BarrierDirection BarrierDir { get; set; }
        public double BarrierLevel { get; set; }

        public BarrierOption(Stock stock, double strike, DateTime expiry, bool isCall, BarrierType barrierOptionType, BarrierDirection barrierDir, double barrierLevel)
        {
            Stock = stock;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            BarrierOptionType = barrierOptionType;
            BarrierDir = barrierDir;
            BarrierLevel = barrierLevel;
        }

        public PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks)
        {
            throw new NotImplementedException();
        }
    }
}