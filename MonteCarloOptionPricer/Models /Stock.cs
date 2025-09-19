using System;
using System.Runtime.InteropServices.Swift;


namespace MonteCarloOptionPricer.Models
{

    public class Stock 
    {
        public string Ticker {get; set;}
        public string Exchange {get; set;}
        public double Price { get; set; }


        public Stock(string ticker, string exchange, double price)
        {
            Ticker = ticker;
            Exchange = exchange;
            Price = price;
        }
        


    }


}