using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Simulation;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    public abstract class OptionBase : IOption
    {
        public double InitialPrice { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        protected OptionBase(double initialPrice, double strike, DateTime expiry, bool isCall)
        {
            InitialPrice = initialPrice;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
        }

        protected double TimeToExpiryYears() => (Expiry - DateTime.Today).TotalDays / 365.0;

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

            // Determine whether to generate paths or terminals
            bool requiresPaths = RequiresPaths();
            var sim = MonteCarloSimulator.Simulate(p, keepPaths: requiresPaths);

            // Pass both terminals and paths to BuildPayoffs
            var payoffs = BuildPayoffs(sim.Terminals, sim.Paths, p);

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

        // Default implementation assumes only terminal prices are needed
        protected virtual bool RequiresPaths() => false;

        // Unified BuildPayoffs method
        protected abstract List<double> BuildPayoffs(List<double> terminals, List<double[]>? paths, SimulationParameters p);

        private void ComputeGreeks(PricingResult res, SimulationParameters p, double basePrice, int seed)
        {
            res.Delta = ComputeBumpedPrice(p, basePrice, seed, eps: 1.0, bump => bump.InitialPrice += 1.0);
            res.Gamma = ComputeGamma(p, basePrice, seed, eps: 1.0);
            res.Vega = ComputeBumpedPrice(p, basePrice, seed, eps: 0.01, bump => bump.Volatility += 0.01);
            res.Rho = ComputeBumpedPrice(p, basePrice, seed, eps: 0.0001, bump => bump.RiskFreeRate += 0.0001);
            res.Theta = ComputeBumpedPrice(p, basePrice, seed, eps: 1.0 / 365.0, bump => bump.TimeToExpiry -= 1.0 / 365.0);
        }

        private double ComputeBumpedPrice(SimulationParameters p, double basePrice, int seed, double eps, Action<SimulationParameters> bumpAction)
        {
            var bumped = Clone(p);
            bumpAction(bumped);

            RandomNumberGenerator.Seed(seed);
            var sim = MonteCarloSimulator.Simulate(bumped, keepPaths: RequiresPaths());
            var payoffs = BuildPayoffs(sim.Terminals, sim.Paths, bumped);

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
            var simUp = MonteCarloSimulator.Simulate(up, keepPaths: RequiresPaths());
            var simDown = MonteCarloSimulator.Simulate(down, keepPaths: RequiresPaths());

            var payoffsUp = BuildPayoffs(simUp.Terminals, simUp.Paths, up);
            var payoffsDown = BuildPayoffs(simDown.Terminals, simDown.Paths, down);

            double df = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);
            double priceUp = df * payoffsUp.Average();
            double priceDown = df * payoffsDown.Average();

            return (priceUp - 2 * basePrice + priceDown) / (eps * eps);
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