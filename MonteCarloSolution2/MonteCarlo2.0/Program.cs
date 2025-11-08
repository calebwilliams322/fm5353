using System;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.OptionModels;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n==============================================");
            Console.WriteLine("  Monte Carlo Option Pricer — Interactive Mode");
            Console.WriteLine("==============================================");
            Console.WriteLine("This program prices different option types using Monte Carlo simulation.\n");
            Console.WriteLine("You'll be prompted to enter key parameters:");
            Console.WriteLine("  • Underlying price, strike, volatility, risk-free rate, maturity");
            Console.WriteLine("  • Option type (European, Asian, Digital, Barrier, Lookback, Range)");
            Console.WriteLine("  • Simulation type (Plain, Antithetic, Control Variate, etc.)");
            Console.WriteLine("  • Whether to compute Greeks and use multithreading");
            Console.WriteLine("\nPress Enter to accept defaults at any promptss.\n");

            // === Core Inputs ===
            double S0 = ReadDouble("Enter initial underlying price (S0):", 100.0);
            double K = ReadDouble("Enter strike/reference price (K):", 100.0);
            double sigma = ReadDouble("Enter volatility (e.g. 0.2 for 20%):", 0.2);
            double r = ReadDouble("Enter risk-free rate (e.g. 0.05 for 5%):", 0.05);
            double T = ReadDouble("Enter time to expiry in years:", 1.0);
            bool isCall = ReadBool("Is this a Call option? (Y/N):", true);
            bool calcGreeks = ReadBool("Calculate Greeks? (Y/N):", true);
            bool useThreads = ReadBool("Enable multithreading? (Y/N):", true);

            // === Additional Inputs for Exotic Options ===
            double barrierLevel = ReadDouble("Enter barrier level (default=110):", 110.0);
            double digitalAmount = ReadDouble("Enter digital option payout amount (default=1.0):", 1.0);

            // === Simulation setup ===
            int steps = ReadInt("Enter number of time steps per path:", 252);
            int paths = ReadInt("Enter number of simulated paths:", 10000);
            int simChoice = ReadInt("Choose simulation mode:\n  1=Plain\n  2=Antithetic\n  3=ControlVariate\n[default=1]:", 1);

            SimulationMode simMode = simChoice switch
            {
                2 => SimulationMode.Antithetic,
                3 => SimulationMode.ControlVariate,
                _ => SimulationMode.Plain
            };

            // === Option type selection ===
            Console.WriteLine("\nSelect option type:");
            Console.WriteLine("  1 = European");
            Console.WriteLine("  2 = Asian");
            Console.WriteLine("  3 = Digital");
            Console.WriteLine("  4 = Barrier");
            Console.WriteLine("  5 = Lookback");
            Console.WriteLine("  6 = Range");
            int optChoice = ReadInt("Enter your choice [default=1]:", 1);

            DateTime expiry = DateTime.Today.AddYears((int)Math.Ceiling(T));

            // === Construct chosen option ===
            IOption selectedOption = optChoice switch
            {
                2 => new AsianOption(S0, K, expiry, isCall, AveragingType.Arithmetic),
                3 => new DigitalOption(S0, K, expiry, isCall, true, digitalAmount),
                4 => new BarrierOption(S0, K, expiry, isCall, BarrierType.KnockOut, BarrierDirection.Up, barrierLevel),
                5 => new LookbackOption(S0, K, expiry, isCall, LookbackType.Max),
                6 => new RangeOption(S0, K, expiry),
                _ => new EuropeanOption(S0, K, expiry, isCall),
            };

            // === Simulation Execution ===
            Console.WriteLine($"\nRunning {simMode} Monte Carlo simulation for {selectedOption.GetType().Name}...");
            var result = selectedOption.GetPrice(
                volatility: sigma,
                riskFreeRate: r,
                timeSteps: steps,
                numberOfPaths: paths,
                calculateGreeks: calcGreeks,
                useMultithreading: useThreads,
                simMode: simMode
            );

            // === Display Results ===
            Console.WriteLine($"\n=== {selectedOption.GetType().Name} Results ===");
            PrintResult(result);

            // === Bonus Comparison Across All Option Types ===
            Console.WriteLine("\n=== Comparative Simulation Across All Option Types ===");
            Console.WriteLine("This uses the same shared parameters to conceptually compare pricing levels.\n");

            IOption[] allOptions = new IOption[]
            {
                new EuropeanOption(S0, K, expiry, isCall),
                new AsianOption(S0, K, expiry, isCall, AveragingType.Arithmetic),
                new DigitalOption(S0, K, expiry, isCall, true, digitalAmount),
                new BarrierOption(S0, K, expiry, isCall, BarrierType.KnockOut, BarrierDirection.Up, barrierLevel),
                new LookbackOption(S0, K, expiry, isCall, LookbackType.Max),
                new RangeOption(S0, K, expiry)
            };

            foreach (var opt in allOptions)
            {
                var res = opt.GetPrice(sigma, r, steps, paths, false, useThreads, simMode);
                Console.WriteLine($"{opt.GetType().Name,-20}  →  Price: {res.Price,10:F4}");
            }

            Console.WriteLine("\nSimulation complete!");
            Console.WriteLine("Observe how exotic features (path-dependence, barriers, averaging) affect value.");
            Console.WriteLine("Higher path sensitivity → generally higher vega and slower convergence.\n");
        }

        // === Helper Methods ===

        static double ReadDouble(string prompt, double defaultValue)
        {
            Console.WriteLine(prompt);
            string? input = Console.ReadLine();
            if (double.TryParse(input, out double val)) return val;
            Console.WriteLine($"Defaulting to {defaultValue}.\n");
            return defaultValue;
        }

        static int ReadInt(string prompt, int defaultValue)
        {
            Console.WriteLine(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int val)) return val;
            Console.WriteLine($"Defaulting to {defaultValue}.\n");
            return defaultValue;
        }

        static bool ReadBool(string prompt, bool defaultValue)
        {
            Console.WriteLine(prompt);
            string? input = Console.ReadLine()?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"Defaulting to {(defaultValue ? "Yes" : "No")}.\n");
                return defaultValue;
            }
            return input switch
            {
                "Y" or "YES" => true,
                "N" or "NO" => false,
                _ => defaultValue
            };
        }

        static void PrintResult(PricingResult res)
        {
            Console.WriteLine($"  Price          : {res.Price,10:F4}");
            if (res.StandardError.HasValue)
                Console.WriteLine($"  Std. Error     : {res.StandardError.Value,10:F4}");
            Console.WriteLine($"  Δ Delta        : {res.Delta,10:F4}");
            Console.WriteLine($"  Γ Gamma        : {res.Gamma,10:F4}");
            Console.WriteLine($"  ν Vega         : {res.Vega,10:F4}");
            Console.WriteLine($"  θ Theta        : {res.Theta,10:F4}");
            Console.WriteLine($"  ρ Rho          : {res.Rho,10:F4}");
        }
    }
}