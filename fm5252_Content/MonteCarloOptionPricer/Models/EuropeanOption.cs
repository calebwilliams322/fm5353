using System;
using System.Collections.Generic;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer.Models
{
    public class EuropeanOption : IOption
    {
        public Stock Stock      { get; set;}
        public double Strike    { get; set;}
        public DateTime Expiry  { get; set;}
        public bool IsCall      { get; set;}

        public EuropeanOption(Stock stock, double strike, DateTime expiry, bool isCall)
        {
            Stock  = stock;
            Strike = strike;
            Expiry = expiry;
            IsCall = isCall;
        }

        private double TimeToExpiryYears() => (Expiry - DateTime.Today).TotalDays / 365.0;

        private PricingParameters ToPricingParameters(
            double volatility,
            double riskFreeRate,
            int timeSteps,
            int numberOfPaths,
            SimulationMode simMode,
            int vdcBase1,
            int vdcBase2,
            int vdcPoints
            )
        {
            return new PricingParameters
            {
                InitialPrice   = Stock.Price,
                Volatility     = volatility,
                RiskFreeRate   = riskFreeRate,
                TimeToExpiry   = TimeToExpiryYears(),
                TimeSteps      = timeSteps,
                NumberOfPaths  = numberOfPaths,
                SimMode       = simMode,
                VdCBase1       = vdcBase1,
                VdCBase2 = vdcBase2,
                VdCPoints     = vdcPoints
            };


        }

        public PricingResult GetPrice(
            double volatility,
            double riskFreeRate,
            int timeSteps,
            int numberOfPaths,
            bool calculateGreeks = false,
            SimulationMode simMode = SimulationMode.Plain,
            int vdcBase1 = 2,
            int vdcBase2 = 5,
            int vdcPoints = 1024)
        {

            var p = new PricingParameters
            {
                InitialPrice  = Stock.Price,
                Volatility    = volatility,
                RiskFreeRate  = riskFreeRate,
                TimeToExpiry  = (Expiry - DateTime.Today).TotalDays / 365.0,
                TimeSteps     = timeSteps,
                NumberOfPaths = numberOfPaths,
                SimMode       = simMode,
                VdCBase1       = vdcBase1,
                VdCBase2      = vdcBase2,
                VdCPoints     = vdcPoints
            };

            // 2) Simulate raw terminal prices
            int seed = new Random().Next();
            NormalGenerator.Seed(seed);
            var terminals = MonteCarloSimulator.SimulateTerminalPrices(p);

            var payoffs = BuildPayoffs(terminals, p);
            
            // 4) Price and standard error
            double avgPayoff = payoffs.Average();
            double df        = Math.Exp(-riskFreeRate * p.TimeToExpiry);
            double price     = df * avgPayoff;
            
            double stderr;
            if (p.SimMode == SimulationMode.VanDerCorput)
            {
                stderr = 0.0;
            }
            else
            {
                stderr = ComputeStandardError(payoffs, avgPayoff, df);
            }

            var result = new PricingResult
            {
                Price         = price,
                StandardError = stderr
            };

            if (calculateGreeks)
                ComputeGreeks(result, p, price, seed);

            return result;
        }

        private double ComputeStandardError(
            List<double> payoffs,
            double avgPayoff,
            double df)
        {
            double sumSq = 0.0;
            foreach (var x in payoffs)
            {
                double discPay = df * x;
                sumSq += (discPay - df * avgPayoff) * (discPay - df * avgPayoff);
            }
            int n = payoffs.Count;
            return Math.Sqrt(sumSq / (n * (n - 1)));
        }


        /// <summary>
        /// Computes all Greeks, recalculating discount factor internally.
        /// </summary>
        private void ComputeGreeks(
            PricingResult result,
            PricingParameters p,
            double basePrice,
            int seed)
        {
            result.Delta = ComputeDelta(p, basePrice, 1, seed);
            result.Gamma = ComputeGamma(p, basePrice, 1, seed);
            result.Vega  = ComputeVega(p, basePrice, 0.01, seed);
            result.Rho   = ComputeRho(p, basePrice, 0.0001, seed);
            result.Theta = ComputeTheta(p, basePrice, 1.0 / 365.0, seed);
        }

        private double ComputeDelta(
            PricingParameters p,
            double basePrice,
            double eps,
            int seed)
        {
            double df = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);
            var bumped = CloneParameters(p);
            bumped.InitialPrice += eps;
            NormalGenerator.Seed(seed);
            var terms = MonteCarloSimulator.SimulateTerminalPrices(bumped);
            double upPayoff = BuildPayoffs(terms, bumped).Average();
            return (df * upPayoff - basePrice) / eps;
        }

        private double ComputeGamma(
            PricingParameters p,
            double basePrice,
            double eps,
            int seed)
        {
            double df = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);

            // bump up and bump down
            var bumpedUp   = CloneParameters(p);
            var bumpedDown = CloneParameters(p);
            bumpedUp.InitialPrice   += eps;
            bumpedDown.InitialPrice -= eps;

            // up path
            NormalGenerator.Seed(seed);
            var termsUp   = MonteCarloSimulator.SimulateTerminalPrices(bumpedUp);
            var payoffsUp = BuildPayoffs(termsUp, bumpedUp);
            double avgUp  = payoffsUp.Average();
            double priceUp = df * avgUp;

            // down path
            NormalGenerator.Seed(seed);
            var termsDown   = MonteCarloSimulator.SimulateTerminalPrices(bumpedDown);
            var payoffsDown = BuildPayoffs(termsDown, bumpedDown);
            double avgDown  = payoffsDown.Average();
            double priceDown = df * avgDown;

            // central‐difference second derivative
            return (priceUp - 2 * basePrice + priceDown) / (eps * eps);
        }

        private double ComputeVega(
            PricingParameters p,
            double basePrice,
            double eps,
            int seed)
        {
            double df = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);

            var bumped = CloneParameters(p);
            bumped.Volatility += eps;

            NormalGenerator.Seed(seed);
            var terms   = MonteCarloSimulator.SimulateTerminalPrices(bumped);
            var payoffs = BuildPayoffs(terms, bumped);
            double avg  = payoffs.Average();
            double priceBumped = df * avg;

            return (priceBumped - basePrice) / eps;
        }

        private double ComputeRho(
            PricingParameters p,
            double basePrice,
            double eps,
            int seed)
        {
            // bump the interest rate
            var bumped = CloneParameters(p);
            bumped.RiskFreeRate += eps;

            NormalGenerator.Seed(seed);
            var terms   = MonteCarloSimulator.SimulateTerminalPrices(bumped);
            var payoffs = BuildPayoffs(terms, bumped);
            double avg  = payoffs.Average();

            // discount with the bumped rate
            double dfBumped = Math.Exp(-bumped.RiskFreeRate * bumped.TimeToExpiry);
            double priceBumped = dfBumped * avg;

            return (priceBumped - basePrice) / eps;
        }

        private double ComputeTheta(
            PricingParameters p,
            double basePrice,
            double eps,
            int seed)
        {
            // bump time to expiry down by eps (in years)
            var bumped = CloneParameters(p);
            bumped.TimeToExpiry -= eps;

            NormalGenerator.Seed(seed);
            var terms   = MonteCarloSimulator.SimulateTerminalPrices(bumped);
            var payoffs = BuildPayoffs(terms, bumped);
            double avg  = payoffs.Average();

            // discount with original rate but new time
            double dfBumped = Math.Exp(-p.RiskFreeRate * bumped.TimeToExpiry);
            double priceBumped = dfBumped * avg;

            // Theta ≈ (P(T–eps) – P(T)) / eps
            return (priceBumped - basePrice) / eps;
        }

        private PricingParameters CloneParameters(PricingParameters p)
        {
            return new PricingParameters
            {
                InitialPrice  = p.InitialPrice,
                Volatility    = p.Volatility,
                RiskFreeRate  = p.RiskFreeRate,
                TimeToExpiry  = p.TimeToExpiry,
                TimeSteps     = p.TimeSteps,
                NumberOfPaths = p.NumberOfPaths,
                SimMode       = p.SimMode,
                VdCBase1       = p.VdCBase1,
                VdCBase2       = p.VdCBase2,
                VdCPoints     = p.VdCPoints
            };
        }

        /// <summary>
        /// Given raw terminal prices (S_T), apply the payoff function,
        /// and—for antithetic mode—collapse each pair into one averaged payoff.
        /// </summary>
        private List<double> BuildPayoffs(
            List<double> terminalPrices,
            PricingParameters p)
        {
            var payoffs = new List<double>();


            // helper payoff function 
            Func<double,double> payoffFn = ST =>
                IsCall
                ? Math.Max(ST - Strike, 0)
                : Math.Max(Strike - ST, 0);


            // Antithetic collapse of the pairs
            if (p.SimMode == SimulationMode.Antithetic)
            {
                int pairCount = terminalPrices.Count / 2;
                for (int i = 0; i < pairCount; i++)
                {
                    double x1 = payoffFn(terminalPrices[2*i]);
                    double x2 = payoffFn(terminalPrices[2*i + 1]);
                    payoffs.Add(0.5 * (x1 + x2));
                }
                
            }
            else
            {
                // Plain or VdC: one payoff per terminal
                payoffs.AddRange(terminalPrices.Select(payoffFn));
            }

            return payoffs;
        }

        public PricingResult GetPrice(double vol, double rate, int steps, int paths, bool calculateGreeks)
        {
            throw new NotImplementedException();
        }
    }
}
