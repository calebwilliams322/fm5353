using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Simulation;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public class LookbackOption : IOption
    {
        public double InitialPrice { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }
        public LookbackType LookbackOptionType { get; set; }

        public LookbackOption(
            double initialPrice,
            double strike,
            DateTime expiry,
            bool isCall,
            LookbackType lookbackType = LookbackType.Max)
        {
            InitialPrice = initialPrice;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            LookbackOptionType = lookbackType;
        }

        private double TimeToExpiryYears() => (Expiry - DateTime.Today).TotalDays / 365.0;

        // === Main Pricing Method ===
        public PricingResult GetPrice(
            double volatility,
            double riskFreeRate,
            int timeSteps,
            int numberOfPaths,
            bool calculateGreeks = false,
            bool useMultithreading = true,
            SimulationMode simMode = SimulationMode.Plain)
        {
            var p = new SimulationParameters
            {
                InitialPrice = InitialPrice,
                Volatility = volatility,
                RiskFreeRate = riskFreeRate,
                TimeToExpiry = TimeToExpiryYears(),
                TimeSteps = timeSteps,
                NumberOfPaths = numberOfPaths,
                UseMultithreading = useMultithreading,
                SimMode = simMode,
                ReferenceStrike = Strike
            };

            int seed = new Random().Next();
            RandomNumberGenerator.Seed(seed);

            // Lookback requires full paths
            var sim = MonteCarloSimulator.Simulate(p, keepPaths: true);
            var payoffs = BuildLookbackPayoffs(sim.Paths!, IsCall, Strike, LookbackOptionType);

            double df = Math.Exp(-riskFreeRate * p.TimeToExpiry);
            double avgPayoff = payoffs.Average();
            double price = df * avgPayoff;

            double stderr = simMode == SimulationMode.VanDerCorput
                ? 0.0
                : ComputeStandardError(payoffs, avgPayoff, df);

            var result = new PricingResult
            {
                Price = price,
                StandardError = stderr
            };

            if (calculateGreeks)
                ComputeGreeks(result, p, price, seed);

            return result;
        }

        // === Greeks ===
        private void ComputeGreeks(PricingResult res, SimulationParameters p, double basePrice, int seed)
        {
            res.Delta = ComputeDelta(p, basePrice, seed, eps: 1.0);
            res.Gamma = ComputeGamma(p, basePrice, seed, eps: 1.0);
            res.Vega = ComputeVega(p, basePrice, seed, eps: 0.01);
            res.Rho = ComputeRho(p, basePrice, seed, eps: 0.0001);
            res.Theta = ComputeTheta(p, basePrice, seed, eps: 1.0 / 365.0);
        }

        private double ComputeDelta(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.InitialPrice += eps;
            RandomNumberGenerator.Seed(seed);

            var sim = MonteCarloSimulator.Simulate(bumped, keepPaths: true);
            var payoffs = BuildLookbackPayoffs(sim.Paths!, IsCall, Strike, LookbackOptionType);

            double df = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();
            return (bumpedPrice - basePrice) / eps;
        }

        private double ComputeGamma(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var up = Clone(p);
            var down = Clone(p);
            up.InitialPrice += eps;
            down.InitialPrice -= eps;

            RandomNumberGenerator.Seed(seed);
            var simUp = MonteCarloSimulator.Simulate(up, keepPaths: true);
            var simDown = MonteCarloSimulator.Simulate(down, keepPaths: true);

            var payoffsUp = BuildLookbackPayoffs(simUp.Paths!, IsCall, Strike, LookbackOptionType);
            var payoffsDown = BuildLookbackPayoffs(simDown.Paths!, IsCall, Strike, LookbackOptionType);

            double df = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);
            double priceUp = df * payoffsUp.Average();
            double priceDown = df * payoffsDown.Average();

            return (priceUp - 2 * basePrice + priceDown) / (eps * eps);
        }

        private double ComputeVega(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.Volatility += eps;
            RandomNumberGenerator.Seed(seed);

            var sim = MonteCarloSimulator.Simulate(bumped, keepPaths: true);
            var payoffs = BuildLookbackPayoffs(sim.Paths!, IsCall, Strike, LookbackOptionType);

            double df = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();
            return (bumpedPrice - basePrice) / eps;
        }

        private double ComputeRho(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.RiskFreeRate += eps;
            RandomNumberGenerator.Seed(seed);

            var sim = MonteCarloSimulator.Simulate(bumped, keepPaths: true);
            var payoffs = BuildLookbackPayoffs(sim.Paths!, IsCall, Strike, LookbackOptionType);

            double df = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();
            return (bumpedPrice - basePrice) / eps;
        }

        private double ComputeTheta(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.TimeToExpiry -= eps;
            RandomNumberGenerator.Seed(seed);

            var sim = MonteCarloSimulator.Simulate(bumped, keepPaths: true);
            var payoffs = BuildLookbackPayoffs(sim.Paths!, IsCall, Strike, LookbackOptionType);

            double df = Math.Exp(-p.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();
            return (bumpedPrice - basePrice) / eps;
        }

        // === Payoff Construction ===
        private static List<double> BuildLookbackPayoffs(List<double[]> paths, bool isCall, double strike, LookbackType lookbackType)
        {
            var payoffs = new List<double>(paths.Count);

            foreach (var path in paths)
            {
                double ST = path[^1];
                double Smax = path.Max();
                double Smin = path.Min();

                double payoff = 0.0;
                if (lookbackType == LookbackType.Max) // Fixed-strike: max over time
                    payoff = isCall ? Math.Max(Smax - strike, 0) : Math.Max(strike - Smin, 0);
                else // Floating-strike variant (not used for now)
                    payoff = isCall ? Math.Max(ST - Smin, 0) : Math.Max(Smax - ST, 0);

                payoffs.Add(payoff);
            }

            return payoffs;
        }

        private static double ComputeStandardError(List<double> payoffs, double avgPayoff, double df)
        {
            double sumSq = payoffs.Sum(x =>
            {
                double disc = df * x;
                return (disc - df * avgPayoff) * (disc - df * avgPayoff);
            });
            int n = payoffs.Count;
            return Math.Sqrt(sumSq / (n * (n - 1)));
        }

        private static SimulationParameters Clone(SimulationParameters p) => new()
        {
            InitialPrice = p.InitialPrice,
            Volatility = p.Volatility,
            RiskFreeRate = p.RiskFreeRate,
            TimeToExpiry = p.TimeToExpiry,
            TimeSteps = p.TimeSteps,
            NumberOfPaths = p.NumberOfPaths,
            SimMode = p.SimMode,
            ReferenceStrike = p.ReferenceStrike,
            UseMultithreading = p.UseMultithreading
        };
    }
}