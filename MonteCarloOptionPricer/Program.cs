// See https://aka.ms/new-console-template for more information

using System;
using System.ComponentModel.Design.Serialization;
using MonteCarloOptionPricer.Models;


namespace MonteCarloOptionPricer
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("=== Welcome to my Monte Carlo European Option Pricer ===");
            Console.WriteLine("This program prices a Euro Option using a basic GBM Monte Carlo engine.");
            Console.WriteLine("You will be guided through entering all the required parameters to...\n" +
                  "  • create an underlying stock object\n" +
                  "  • design a Euro Option derived from this stock\n" +
                  "  • generate a simulation using\n"  + 
                  "    - risk free rate\n" + 
                  "    - realized vol\n" + 
                  "    - number of steps\n" + 
                  "    - number of paths\n");


            Console.WriteLine();

            // Read and validate inputs
            string ticker = ReadString("Enter the underlying stock ticker:", "AAPL");
            double s0 = ReadDouble("Enter initial stock price (S0):", 100.0);
            double strike = ReadDouble("Enter strike price (K):", 100.0);
            double maturity = ReadDouble("Enter time to expiry in years (T):", 1.0);
            bool isCall = ReadBool("Call option? (Y/N):", true);
            bool calculateGreeks = ReadBool("Do you want to calculate the Greeks? (Y/N): ", true);
            double volatility = ReadDouble("Enter volatility (e.g., 0.2 for 20%):", 0.2);
            double riskFreeRate = ReadDouble("Enter risk-free rate (e.g., 0.05 for 5%):", 0.05);
            int timeSteps = ReadInt("Enter number of time steps per path:", 100);
            int numberOfPaths = ReadInt("Enter number of simulation paths:", 10000);
            int simChoice = ReadInt("Choose simulation mode (1=Plain, 2=Antithetic, 3=Van der Corput, 4=ControlVariate, 5=Antithetic and ControlVariate) [default=1]:",
                    1);

                SimulationMode simMode;
                switch (simChoice)
                {
                    case 2:
                        simMode = SimulationMode.Antithetic;
                        break;
                    case 3:
                        simMode = SimulationMode.VanDerCorput;
                        break;
                    case 4:
                        simMode = SimulationMode.ControlVariate;
                        break;
                    case 5:
                        simMode = SimulationMode.Antithetic_and_ControlVariate;
                        break;
                    default:
                        simMode = SimulationMode.Plain;
                        break;
                        
                }
            
            int vdcBase1   = 2;
            int vdcBase2 = 5;
            int vdcPoints = 1024;
            if (simMode == SimulationMode.VanDerCorput)
            {
                vdcBase1   = ReadInt("Van der Corput base (b > 1) [default=5]:",    5);
                vdcBase2   = ReadInt("Van der Corput base (b > 1) [default=7]:",    7);

                vdcPoints = ReadInt("Number of VdC sequence points [default=1000]:", 1000);
            }
            

            // Build objects
            var stock = new Stock(ticker, "", s0);
            DateTime expiryDate = DateTime.Today.AddDays(maturity * 365.0);
            var option = new EuropeanOption(stock, strike, expiryDate, isCall);

            // Price the option
            var result = option.GetPrice(
                volatility,
                riskFreeRate,
                timeSteps,
                numberOfPaths,
                calculateGreeks,
                simMode,
                vdcBase1,
                vdcBase2,
                vdcPoints 
            );

            // print the core price + error
            Console.WriteLine($"\n{option.Stock.Ticker} Option price: {result.Price:F4}");
            if (simMode != SimulationMode.VanDerCorput)
            {
                Console.WriteLine($"Standard error: {result.StandardError:F4}");
            }
            


            // if you asked for Greeks, display them:
            if (result.Delta.HasValue)
            {
                Console.WriteLine($"Delta: {result.Delta:F4}");
                Console.WriteLine($"Gamma: {result.Gamma:F4}");
                Console.WriteLine($"Vega:  {result.Vega:F4}");
                Console.WriteLine($"Theta: {result.Theta:F4}");
                Console.WriteLine($"Rho:   {result.Rho:F4}");
            }

        }

        // ----- Helper methods -----

        static string ReadString(string prompt, string defaultValue)
        {
            Console.WriteLine(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"No input detected. Defaulting to '{defaultValue}'.");
                return defaultValue;
            }
            return input.Trim();
        }

        static double ReadDouble(string prompt, double defaultValue)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine($"No input detected. Defaulting to {defaultValue}.");
                    return defaultValue;
                }
                if (double.TryParse(input, out double result))
                    return result;
                Console.WriteLine("Invalid number. Please enter a numeric value.");
            }
        }

        static int ReadInt(string prompt, int defaultValue)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine($"No input detected. Defaulting to {defaultValue}.");
                    return defaultValue;
                }
                if (int.TryParse(input, out int result))
                    return result;
                Console.WriteLine("Invalid integer. Please enter a whole number.");
            }
        }

        static bool ReadBool(string prompt, bool defaultValue)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine($"No input detected. Defaulting to {(defaultValue ? "Yes" : "No")}.\n");
                    return defaultValue;
                }
                input = input.Trim().ToUpper();
                if (input == "Y" || input == "YES")
                    return true;
                if (input == "N" || input == "NO")
                    return false;
                Console.WriteLine("Invalid input. Please type 'Y' for yes or 'N' for no.");
            }
        }
    }
}
