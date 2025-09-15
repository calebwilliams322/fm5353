using System;
using MonteCarloOptionPricer.Models;


namespace MonteCarloOptionPricer.Models
{

    public interface IOption
    {

        // Most general characteristics 
        double Strike {get; set;}
        DateTime Expiry {get; set;}
        Stock Stock {get; set;}
        bool IsCall {get; set;}

    

        // You should be able to price the option based on it's own characteristics
        PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks);
        

    }


}