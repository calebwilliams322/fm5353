using System;
using System.Linq;
using MonteCarloOptionPricer.Models;


namespace MonteCarloOptionPricer.Simulation
{

    /// <summary>
    /// Monte Carlo simulator for generating terminal asset prices
    /// Uses NormalGenerator for random normals.
    /// </summary>
    /// 

    public static class MonteCarloSimulator
    {
        /// <summary>
        /// Simulates terminal asset prices/paths at/to expiry.
        /// </summary>
        /// <param name="parameters">Simulation settings.</param>
        /// <returns>List of simulated final prices or all simulated asset paths.</returns>
        /// 


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
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameters.SimMode), parameters.SimMode, "Unsupported simulation mode");
            }
        }

        
        // Simulates plain final prices
        private static List<double> SimulatePlainPrices(PricingParameters parameters)
        {
            var terminals = new List<double>(parameters.NumberOfPaths);
            double dt = parameters.TimeToExpiry / parameters.TimeSteps;

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                double s = parameters.InitialPrice;
                for (int step = 0; step < parameters.TimeSteps; step++)
                {
                    double z = NormalGenerator.NextStandardNormal();
                    s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                                   + parameters.Volatility * Math.Sqrt(dt) * z);
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

                for (int step = 0; step < parameters.TimeSteps; step++)
                {
                    double z = NormalGenerator.NextStandardNormal();
                    double drift = (parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt;
                    double diffusion = parameters.Volatility * Math.Sqrt(dt);

                    sPlus  *= Math.Exp(drift + diffusion * z);
                    sMinus *= Math.Exp(drift - diffusion * z);
                }

                // Add both terminals; collapse into payoffs externally
                terminals.Add(sPlus);
                terminals.Add(sMinus);
            }

            return terminals;
        }



        private static List<double> SimulateVdCPrices(PricingParameters p)
        {
            int    N          = p.VdCPoints;
            double S0         = p.InitialPrice;
            double T          = p.TimeToExpiry;
            double r          = p.RiskFreeRate;
            double sigma      = p.Volatility;

            // precompute drift & diffusion
            double drift     = (r - 0.5 * sigma * sigma) * T;
            double diffusion = sigma * Math.Sqrt(T);

            var terminals = new List<double>(2 * N);

            // generate N pairs (z1,z2) via two VdC sequences
            for (int i = 1; i <= N; i++)
            {
                // get two quasi‐uniforms from different bases
                double u = VanDerCorput(i, p.VdCBase1);
                double v = VanDerCorput(i, p.VdCBase2);

                // Box–Muller transform
                double R     = Math.Sqrt(-2.0 * Math.Log(u));
                double theta = 2.0 * Math.PI * v;
                double z1    = R * Math.Cos(theta);
                double z2    = R * Math.Sin(theta);

                // push two terminal prices
                terminals.Add(S0 * Math.Exp(drift + diffusion * z1));
                terminals.Add(S0 * Math.Exp(drift + diffusion * z2));
            }

            return terminals;
        }

        


        // returns full asset paths
        public static List<double[]> SimulateAssetPaths(PricingParameters parameters)
        {
            var paths = new List<double[]>(parameters.NumberOfPaths);
            int steps = parameters.TimeSteps;
            double dt = parameters.TimeToExpiry / steps;

            for (int path = 0; path < parameters.NumberOfPaths; path++)
            {
                // create an array for each path: index 0 = initial price, then each step
                var prices = new double[steps + 1];
                prices[0] = parameters.InitialPrice;
                double s = parameters.InitialPrice;

                for (int step = 1; step <= steps; step++)
                {
                    double z = NormalGenerator.NextStandardNormal();
                    s *= Math.Exp((parameters.RiskFreeRate - 0.5 * parameters.Volatility * parameters.Volatility) * dt
                                   + parameters.Volatility * Math.Sqrt(dt) * z);
                    prices[step] = s;
                }

                paths.Add(prices);
            }

            return paths;
        }



        /// <summary>
        /// Computes the i-th element of the 1-D Van der Corput sequence in base b.
        /// </summary>
        private static double VanDerCorput(int index, int b)
        {
            double result = 0.0;
            double denom  = 1.0;

            // Reflect base-b digits across the decimal point
            while (index > 0)
            {
                denom   /= b; 
                int digit = index % b;
                result  += digit * denom;
                index   /= b;
            }

            return result;  // in [0,1)
        }





    }


}