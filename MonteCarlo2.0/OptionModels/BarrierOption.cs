using System;
using System.Collections.Generic;
using System.Linq;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer.OptionModels
{
    /// <summary>
    /// Represents a path-dependent barrier option (knock-in or knock-out),
    /// supporting both call and put payoffs with up or down barriers.
    /// </summary>
    public class BarrierOption : IOption
    {
        // === Core fields (required by IOption) ===
        public double InitialPrice { get; set; }
        public double Strike { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsCall { get; set; }

        // === Barrier-specific fields ===
        public BarrierType BarrierOptionType { get; set; }    // KnockIn / KnockOut
        public BarrierDirection BarrierDir { get; set; }      // Up / Down
        public double BarrierLevel { get; set; }              // Barrier price level

        public BarrierOption(
            double initialPrice,
            double strike,
            DateTime expiry,
            bool isCall,
            BarrierType barrierType,
            BarrierDirection barrierDir,
            double barrierLevel)
        {
            InitialPrice = initialPrice;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
            BarrierOptionType = barrierType;
            BarrierDir = barrierDir;
            BarrierLevel = barrierLevel;
        }

        private double TimeToExpiryYears() =>
            (Expiry - DateTime.Today).TotalDays / 365.0;

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

            // Barrier requires full paths
            // Barrier requires full simulated paths
            var sim = MonteCarloSimulator.Simulate(p, keepPaths: true);

            // Defensive check (compile-time + runtime safety)
            if (sim.Paths == null || sim.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption requires full path data. Ensure keepPaths=true in simulation.");

            var payoffs = BuildBarrierPayoffs(
                paths: sim.Paths!,
                isCall: IsCall,
                strike: Strike,
                barrierType: BarrierOptionType,
                barrierDir: BarrierDir,
                barrierLevel: BarrierLevel);
    

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

        // === Greeks Section ===
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

            if (sim.Paths == null || sim.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption Greek calculation requires full path data (keepPaths=true).");

                        var payoffs = BuildBarrierPayoffs(
                            paths: sim.Paths!,
                            isCall: IsCall,
                            strike: Strike,
                            barrierType: BarrierOptionType,
                            barrierDir: BarrierDir,
                            barrierLevel: BarrierLevel);
    

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

            // --- Up bump ---
            RandomNumberGenerator.Seed(seed);
            var simUp = MonteCarloSimulator.Simulate(up, keepPaths: true);
            if (simUp.Paths == null || simUp.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption Gamma calculation requires full path data (keepPaths=true).");

            var payoffsUp = BuildBarrierPayoffs(
                paths: simUp.Paths!,
                isCall: IsCall,
                strike: Strike,
                barrierType: BarrierOptionType,
                barrierDir: BarrierDir,
                barrierLevel: BarrierLevel);

            // --- Down bump ---
            RandomNumberGenerator.Seed(seed);
            var simDown = MonteCarloSimulator.Simulate(down, keepPaths: true);
            if (simDown.Paths == null || simDown.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption Gamma calculation requires full path data (keepPaths=true).");

            var payoffsDown = BuildBarrierPayoffs(
                paths: simDown.Paths!,
                isCall: IsCall,
                strike: Strike,
                barrierType: BarrierOptionType,
                barrierDir: BarrierDir,
                barrierLevel: BarrierLevel);

            // --- Compute central-difference second derivative ---
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

            if (sim.Paths == null || sim.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption Vega calculation requires full path data (keepPaths=true).");

            var payoffs = BuildBarrierPayoffs(
                paths: sim.Paths!,
                isCall: IsCall,
                strike: Strike,
                barrierType: BarrierOptionType,
                barrierDir: BarrierDir,
                barrierLevel: BarrierLevel);

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

            if (sim.Paths == null || sim.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption Rho calculation requires full path data (keepPaths=true).");

            var payoffs = BuildBarrierPayoffs(
                paths: sim.Paths!,
                isCall: IsCall,
                strike: Strike,
                barrierType: BarrierOptionType,
                barrierDir: BarrierDir,
                barrierLevel: BarrierLevel);

            // Discount using bumped rate
            double df = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();

            return (bumpedPrice - basePrice) / eps;
        }


        private double ComputeTheta(SimulationParameters p, double basePrice, int seed, double eps)
        {
            var bumped = Clone(p);
            bumped.TimeToExpiry -= eps; // reduce time to expiry by eps (e.g., 1 day)

            RandomNumberGenerator.Seed(seed);
            var sim = MonteCarloSimulator.Simulate(bumped, keepPaths: true);

            if (sim.Paths == null || sim.Paths.Count == 0)
                throw new InvalidOperationException("BarrierOption Theta calculation requires full path data (keepPaths=true).");

            var payoffs = BuildBarrierPayoffs(
                paths: sim.Paths!,
                isCall: IsCall,
                strike: Strike,
                barrierType: BarrierOptionType,
                barrierDir: BarrierDir,
                barrierLevel: BarrierLevel);

            // Discount with original rate but shortened time
            double df = Math.Exp(-p.RiskFreeRate * bumped.TimeToExpiry);
            double bumpedPrice = df * payoffs.Average();

            // Theta ≈ (P(T - eps) - P(T)) / eps
            return (bumpedPrice - basePrice) / eps;
        }


        // === Payoff Logic ===
        private static List<double> BuildBarrierPayoffs(
            List<double[]> paths,
            bool isCall,
            double strike,
            BarrierType barrierType,
            BarrierDirection barrierDir,
            double barrierLevel)
        {
            var payoffs = new List<double>(paths.Count);

            foreach (var path in paths)
            {
                bool barrierHit = barrierDir switch
                {
                    BarrierDirection.Up => path.Any(S => S >= barrierLevel),
                    BarrierDirection.Down => path.Any(S => S <= barrierLevel),
                    _ => false
                };

                bool isKnockIn = (barrierType == BarrierType.KnockIn);
                bool isKnockOut = (barrierType == BarrierType.KnockOut);

                bool active =
                    (isKnockIn && barrierHit) ||
                    (isKnockOut && !barrierHit);

                if (!active)
                {
                    payoffs.Add(0.0);
                    continue;
                }

                double ST = path[^1]; // final price
                double payoff = isCall
                    ? Math.Max(ST - strike, 0)
                    : Math.Max(strike - ST, 0);

                payoffs.Add(payoff);
            }

            return payoffs;
        }

        private static double ComputeStandardError(List<double> payoffs, double avg, double df)
        {
            double sumSq = payoffs.Sum(x =>
            {
                double disc = df * x;
                double diff = disc - df * avg;
                return diff * diff;
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