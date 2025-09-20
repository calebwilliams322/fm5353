using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using MonteCarloOptionPricer.Models;


namespace MonteCarloOptionPricer.Simulation
{

    public static class MonteCarloSimulator
    {
        /// <summary>
        /// Simulates terminal asset prices and respective paths using GBM
        /// </summary>
        /// <param name="parameters">Initial asset price.</param>
        /// <returns>Simulated terminal asset prices or full simulated paths</returns>

        /// <summary>
        /// Dispatches simulation based on the specified SimulationMode.
        /// </summary>
        public static List<double> SimulateTerminalPrices(PricingParameters parameters)
        {
            switch (parameters.SimMode)
            {
                case SimulationMode.Plain:
                    return SimulatePlainPrices(parameters);
                case SimulationMode.Antithetic:
                    return SimulateAntitheticPrices(parameters);
                case SimulationMode.VanDerCorput:
                    return SimulateVdCPrices(parameters);
                case SimulationMode.ControlVariate:
                    var (terminalPrices, _) = SimulateControlVariateTerminals(parameters);
                    return terminalPrices;
                case SimulationMode.Antithetic_and_ControlVariate:
                    var (terminalPrices2, _) = SimulateAntitheticControlVariateTerminals(parameters);
                    return terminalPrices2;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameters.SimMode), parameters.SimMode, "Unsupported simulation mode");
            }
        }




        // Simulate final prices using only Box Muller methods ('Plain' method)
        // Utilizes both numbers in the tuple returned by NextTwoStandardNormals

        private static List<double> SimulatePlainPrices(PricingParameters parameters)
        {
            var terminals = new List<double>(parameters.NumberOfPaths);
            double dt = parameters.TimeToExpiry / parameters.TimeSteps;

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                double s = parameters.InitialPrice;
                int step = 0;
                while (step < parameters.TimeSteps)
                {
                    var (z1, z2) = RandomNumberGenerator.NextTwoStandardNormals();

                    // Use z1 for this step
                    s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                              + parameters.Volatility * Math.Sqrt(dt) * z1);
                    step++;

                    // If another step remains, use z2
                    if (step < parameters.TimeSteps)
                    {
                        s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                                      + parameters.Volatility * Math.Sqrt(dt) * z2);
                        step++;
                    }
                }
                terminals.Add(s);
            }
            return terminals;
        }




        /// <summary> 
        /// Simulates terminal asset prices using antithetic variates. 
        /// Returns raw terminal prices (both halves) in sequence; collapsing is handled outside.
        /// </summary>

        private static List<double> SimulateAntitheticPrices(PricingParameters parameters)
        {
            int totalPaths = parameters.NumberOfPaths;
            var terminals = new List<double>(totalPaths);
            double dt = parameters.TimeToExpiry / parameters.TimeSteps;

            for (int i = 0; i < totalPaths; i++)
            {
                double sPlus = parameters.InitialPrice;
                double sMinus = parameters.InitialPrice;

                int step = 0;
                while (step < parameters.TimeSteps)
                {
                    var (z1, z2) = RandomNumberGenerator.NextTwoStandardNormals();
                    double drift = (parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt;

                    // Use z1 for this step
                    double diffusion1 = parameters.Volatility * Math.Sqrt(dt) * z1;
                    sPlus *= Math.Exp(drift + diffusion1);
                    sMinus *= Math.Exp(drift - diffusion1);
                    step++;

                    // If another step remains, use z2
                    if (step < parameters.TimeSteps)
                    {
                        double diffusion2 = parameters.Volatility * Math.Sqrt(dt) * z2;
                        sPlus *= Math.Exp(drift + diffusion2);
                        sMinus *= Math.Exp(drift - diffusion2);
                        step++;
                    }
                }
                terminals.Add(sPlus);
                terminals.Add(sMinus);
            }
            return terminals;
        }



        /// <summary> 
        /// Creates 'pseudo simulation' using Van der Corput sequences for low-discrepancy sampling.
        /// <summary>

        private static List<double> SimulateVdCPrices(PricingParameters p)
        {
            int N = p.VdCPoints;
            double S0 = p.InitialPrice;
            double T = p.TimeToExpiry;
            double r = p.RiskFreeRate;
            double sigma = p.Volatility;

            // precompute drift & diffusion
            double drift = (r - 0.5 * sigma * sigma) * T;
            double diffusion = sigma * Math.Sqrt(T);

            var terminals = new List<double>(2 * N);

            // generate N pairs (z1,z2) via two VdC sequences
            for (int i = 1; i <= N; i++)
            {
                // get two quasi‐uniforms from different bases
                double u = VanDerCorput(i, p.VdCBase1);
                double v = VanDerCorput(i, p.VdCBase2);

                // Box–Muller transform
                double R = Math.Sqrt(-2.0 * Math.Log(u));
                double theta = 2.0 * Math.PI * v;
                double z1 = R * Math.Cos(theta);
                double z2 = R * Math.Sin(theta);

                // push two terminal prices
                terminals.Add(S0 * Math.Exp(drift + diffusion * z1));
                terminals.Add(S0 * Math.Exp(drift + diffusion * z2));
            }

            return terminals;
        }




        public static (List<double> terminalPrices, List<double> pnlHedges) SimulateControlVariateTerminals(PricingParameters parameters)
        {
            var terminalPrices = new List<double>(parameters.NumberOfPaths);
            var pnlHedges = new List<double>(parameters.NumberOfPaths);
            double dt = parameters.TimeToExpiry / parameters.TimeSteps;
            double discount = Math.Exp(-parameters.RiskFreeRate * parameters.TimeToExpiry);

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                double s = parameters.InitialPrice;
                double pnl;
                double cash;
                double t = 0.0;
                double deltaPrev = BlackScholesDelta(s, t, parameters);

                // Initial hedge: short delta shares, invest proceeds at risk-free rate
                cash = -deltaPrev * s;

                for (int step = 1; step <= parameters.TimeSteps; step++)
                {
                    var z = RandomNumberGenerator.NextStandardNormal();
                    s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                                  + parameters.Volatility * Math.Sqrt(dt) * z);

                    t = step * dt;
                    double delta = BlackScholesDelta(s, t, parameters);


                    // Rebalance hedge: buy/sell shares, adjust cash
                    double dDelta = delta - deltaPrev;
                    cash -= dDelta * s;

                    deltaPrev = delta;
                }

                // At expiry: unwind hedge
                cash += deltaPrev * s; // Close hedge
                pnl = cash; // This is the PnL of the hedged portfolio (option payoff to be added outside)
                terminalPrices.Add(s);
                pnlHedges.Add(discount * pnl);
            }
  
            return (terminalPrices, pnlHedges);
        }




        /// <summary>
        /// Utilizes antithetic sampling and control variate techniques to minimize variance
        /// <sumary>
        public static (List<double> terminalPrices, List<double> pnlHedges) SimulateAntitheticControlVariateTerminals(PricingParameters parameters)
         {
            var terminalPrices = new List<double>(parameters.NumberOfPaths);
            var pnlHedges = new List<double>(parameters.NumberOfPaths);

            int steps = parameters.TimeSteps;
            double dt = parameters.TimeToExpiry / steps;
            double driftStep = (parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt;
            double volSqrtDt = parameters.Volatility * Math.Sqrt(dt);
            double discount = Math.Exp(-parameters.RiskFreeRate * parameters.TimeToExpiry);

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                double s = parameters.InitialPrice;
                double t = 0.0;
                double deltaPrev = BlackScholesDelta(s, t, parameters);

                // Initial hedge: short delta shares, invest proceeds at risk-free rate
                double cash = -deltaPrev * s;

                int step = 1;
                while (step <= steps)
                {
                    // Get an antithetic pair
                    var (z1, z2) = RandomNumberGenerator.NextTwoStandardNormals();

                    // ---- Step using z1 ----
                    s *= Math.Exp(driftStep + volSqrtDt * z1);
                    t = step * dt;

                    double delta = BlackScholesDelta(s, t, parameters);
                    double dDelta = delta - deltaPrev;
                    cash -= dDelta * s;
                    deltaPrev = delta;

                    step++;

                    // ---- If another step remains, use z2 immediately ----
                    if (step <= steps)
                    {
                        s *= Math.Exp(driftStep + volSqrtDt * z2);
                        t = step * dt;

                        delta = BlackScholesDelta(s, t, parameters);
                        dDelta = delta - deltaPrev;
                        cash -= dDelta * s;
                        deltaPrev = delta;

                        step++;
                    }
                }

                // Unwind hedge at expiry
                cash += deltaPrev * s;     // close hedge
                double pnl = cash;         // hedged-portfolio PnL (option payoff added outside if desired)

                terminalPrices.Add(s);
                pnlHedges.Add(discount * pnl);
            }

            return (terminalPrices, pnlHedges);
        }

        





        ///
        /// 
        /// ASSET PATHS SECTION (for path dependent options)
        /// 
        /// 





        /// Deploy asset paths code based on simulation mode 
        public static List<double[]> SimulateAssetPaths(PricingParameters parameters)
        {
            switch (parameters.SimMode)
            {
                case SimulationMode.Plain:
                    return SimulatePlainAssetPaths(parameters);
                case SimulationMode.Antithetic:
                    return SimulateAntitheticAssetPaths(parameters);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameters.SimMode), parameters.SimMode, "Unsupported simulation mode");
            }
        }

        // Plain path simulation
        private static List<double[]> SimulatePlainAssetPaths(PricingParameters parameters)
        {
            var paths = new List<double[]>(parameters.NumberOfPaths);
            int steps = parameters.TimeSteps;
            double dt = parameters.TimeToExpiry / steps;

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                var prices = new double[steps + 1];
                prices[0] = parameters.InitialPrice;
                double s = parameters.InitialPrice;

                int step = 1;
                while (step <= steps)
                {
                    var (z1, z2) = RandomNumberGenerator.NextTwoStandardNormals();

                    // Use z1 for this step
                    s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                                   + parameters.Volatility * Math.Sqrt(dt) * z1);
                    prices[step] = s;
                    step++;

                    // If another step remains, use z2
                    if (step <= steps)
                    {
                        s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                                       + parameters.Volatility * Math.Sqrt(dt) * z2);
                        prices[step] = s;
                        step++;
                    }
                }
                paths.Add(prices);
            }
            return paths;
        }



        private static List<double[]> SimulateAntitheticAssetPaths(PricingParameters parameters)
        {
            var paths = new List<double[]>(parameters.NumberOfPaths * 2); // Each simulation produces two paths
            int steps = parameters.TimeSteps;
            double dt = parameters.TimeToExpiry / steps;

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                var pricesPlus = new double[steps + 1];
                var pricesMinus = new double[steps + 1];
                pricesPlus[0] = parameters.InitialPrice;
                pricesMinus[0] = parameters.InitialPrice;
                double sPlus = parameters.InitialPrice;
                double sMinus = parameters.InitialPrice;


                int step = 1;
                while (step <= steps)
                {
                    var (z1, z2) = RandomNumberGenerator.NextTwoStandardNormals();
                    double drift = (parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt;

                    // Use z1 for this step
                    double diffusion1 = parameters.Volatility * Math.Sqrt(dt) * z1;
                    sPlus *= Math.Exp(drift + diffusion1);
                    sMinus *= Math.Exp(drift - diffusion1);
                    pricesPlus[step] = sPlus;
                    pricesMinus[step] = sMinus;
                    step++;

                    // If another step remains, use z2
                    if (step <= steps)
                    {
                        double diffusion2 = parameters.Volatility * Math.Sqrt(dt) * z2;
                        sPlus *= Math.Exp(drift + diffusion2);
                        sMinus *= Math.Exp(drift - diffusion2);
                        pricesPlus[step] = sPlus;
                        pricesMinus[step] = sMinus;
                        step++;
                    }
                }
                paths.Add(pricesPlus);
                paths.Add(pricesMinus);
            }
            return paths;
        }







        ///
        /// 
        /// 
        ///  Helper Functions 
        /// 
        /// 
        /// 






        /// <summary>
        /// Computes the i-th element of the 1-D Van der Corput sequence in base b.
        /// </summary>
        private static double VanDerCorput(int index, int b)
        {
            double result = 0.0;
            double denom = 1.0;

            // Reflect base-b digits across the decimal point
            while (index > 0)
            {
                denom /= b;
                int digit = index % b;
                result += digit * denom;
                index /= b;
            }

            return result;  // in [0,1)
        }


        /// <summary>
        /// Normal CDF approximation  (rational approximation by Abramowitz-Stegun)
        /// </summary>
        private static double NormalCdf(double x)
        {
            // Symmetry for numerical stability: Φ(-x) = 1 − Φ(x)
            if (x < 0) return 1.0 - NormalCdf(-x);

            // Coefficients (A&S 26.2.17–style)
            double p = 0.2316419;
            double b1 = 0.319381530;
            double b2 = -0.356563782;
            double b3 = 1.781477937;
            double b4 = -1.821255978;
            double b5 = 1.330274429;

            double t = 1.0 / (1.0 + p * x);
            double pdf = Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);
            double poly = (((((b5 * t + b4) * t) + b3) * t + b2) * t + b1) * t;

            return 1.0 - pdf * poly;
        }


        /// <summary>
        /// Use D1 to grab option delta
        /// <summary>
        private static double BlackScholesDelta(double S, double t, PricingParameters parameters)
        {
            double T = parameters.TimeToExpiry;   // years
            double K = parameters.Strike;
            double r = parameters.RiskFreeRate;
            double sigma = parameters.Volatility;
            bool isCall = parameters.isCall;

            double tau = T - t;                       // time remaining
            if (tau <= 1e-12)
                return isCall ? (S > K ? 1.0 : 0.0)
                              : (S < K ? -1.0 : 0.0);

            double denom = sigma * Math.Sqrt(tau);
            if (denom <= 0)
                return isCall ? (S >= K ? 1.0 : 0.0)
                              : (S <= K ? -1.0 : 0.0);

            double d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * tau) / denom;

            // Call: Φ(d1); Put: Φ(d1) − 1
            return isCall ? NormalCdf(d1) : NormalCdf(d1) - 1.0;
        }





    }   
}