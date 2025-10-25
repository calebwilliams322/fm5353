using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Simulation;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.OptionModels
{
    /// <summary>
    /// Represents a Digital (Binary) Option — either cash-or-nothing or asset-or-nothing.
    /// Pays a fixed amount if the underlying ends above (for a call) or below (for a put) the strike.
    /// </summary>
    public class DigitalOption : IOption
    {
        // === Core option parameters ===
        public double InitialPrice { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // === Digital-specific fields ===
        public bool IsCashOrNothing { get; set; } = true;
        public double Payout { get; set; } = 1.0;  // payout amount if condition is met

        // --- Constructor ---
        public DigitalOption(
            double initialPrice,
            double strike,
            DateTime expiry,
            bool isCall,
            bool isCashOrNothing = true,
            double payout = 1.0)
        {
            InitialPrice = initialPrice;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            IsCashOrNothing = isCashOrNothing;
            Payout = payout;
        }

        private double TimeToExpiryYears() => (Expiry - DateTime.Today).TotalDays / 365.0;

        // === Pricing method ===
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

            var sim = MonteCarloSimulator.Simulate(p);
            var payoffs = BuildPayoffs(sim.Terminals, IsCall, Strike, IsCashOrNothing, Payout, simMode);

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

        // === Digital Option Payoff Function ===
        private static List<double> BuildPayoffs(
            List<double> terminals,
            bool isCall,
            double strike,
            bool isCashOrNothing,
            double payout,
            SimulationMode mode)
        {
            Func<double, double> payoff;

            if (isCashOrNothing)
            {
                // Pays a fixed cash amount if condition is met
                payoff = ST =>
                    isCall
                    ? (ST > strike ? payout : 0.0)
                    : (ST < strike ? payout : 0.0);
            }
            else
            {
                // Pays asset value if condition is met
                payoff = ST =>
                    isCall
                    ? (ST > strike ? ST : 0.0)
                    : (ST < strike ? ST : 0.0);
            }

            if (mode == SimulationMode.Antithetic)
            {
                var payoffs = new List<double>(terminals.Count / 2);
                for (int i = 0; i < terminals.Count / 2; i++)
                {
                    double a = payoff(terminals[2 * i]);
                    double b = payoff(terminals[2 * i + 1]);
                    payoffs.Add(0.5 * (a + b));
                }
                return payoffs;
            }

            return terminals.Select(payoff).ToList();
        }

        // === Greek Computation (finite difference) ===
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
            var sim = MonteCarloSimulator.Simulate(bumped);
            var payoffs = BuildPayoffs(sim.Terminals, IsCall, Strike, IsCashOrNothing, Payout, bumped.SimMode);

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
            var simUp = MonteCarloSimulator.Simulate(up);
            var simDown = MonteCarloSimulator.Simulate(down);

            var payoffsUp = BuildPayoffs(simUp.Terminals, IsCall, Strike, IsCashOrNothing, Payout, up.SimMode);
            var payoffsDown = BuildPayoffs(simDown.Terminals, IsCall, Strike, IsCashOrNothing, Payout, down.SimMode);

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
            var sim = MonteCarloSimulator.Simulate(bumped);
            var payoffs = BuildPayoffs(sim.Terminals, IsCall, Strike, IsCashOrNothing, Payout, bumped.SimMode);

            double df = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();

            return (bumpedPrice - basePrice) / eps;
        }

        private double ComputeRho(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.RiskFreeRate += eps;

            RandomNumberGenerator.Seed(seed);
            var sim = MonteCarloSimulator.Simulate(bumped);
            var payoffs = BuildPayoffs(sim.Terminals, IsCall, Strike, IsCashOrNothing, Payout, bumped.SimMode);

            double dfBumped = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = dfBumped * payoffs.Average();

            return (bumpedPrice - basePrice) / eps;
        }

        private double ComputeTheta(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.TimeToExpiry -= eps;

            RandomNumberGenerator.Seed(seed);
            var sim = MonteCarloSimulator.Simulate(bumped);
            var payoffs = BuildPayoffs(sim.Terminals, IsCall, Strike, IsCashOrNothing, Payout, bumped.SimMode);

            double df = Math.Exp(-p.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();

            return (bumpedPrice - basePrice) / eps;
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