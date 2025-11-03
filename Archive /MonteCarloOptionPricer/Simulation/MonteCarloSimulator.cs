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

        private static List<double> SimulatePlainPrices(PricingParameters p)
        {
            int nPaths = p.NumberOfPaths;
            int nSteps = p.TimeSteps;
            double dt = p.TimeToExpiry / nSteps;

            //  Pre-generate all random numbers in a single thread
            //    This guarantees thread safety and reproducibility.
            double[,] normals = new double[nPaths, nSteps];
            for (int i = 0; i < nPaths; i++)
            {
                for (int j = 0; j < nSteps; j++)
                {
                    normals[i, j] = RandomNumberGenerator.NextStandardNormal();
                }
            }

            // Prepare storage for terminal prices
            var terminals = new double[nPaths];

            if (p.UseMultithreading)
            {
                Console.WriteLine(" ");
                Console.WriteLine($"[INFO] Running multithreaded simulation on {Environment.ProcessorCount} cores...");

                // --- Multithreaded execution ---
                int numThreads = Math.Max(1, Math.Min(Environment.ProcessorCount, nPaths));
                int baseCount = nPaths / numThreads;
                int remainder = nPaths % numThreads;

                var tasks = new List<Task>(numThreads);
                int offset = 0;

                for (int t = 0; t < numThreads; t++)
                {
                    int count = baseCount + (t < remainder ? 1 : 0);
                    int start = offset;
                    int end = start + count;
                    offset = end;

                    // each thread uses its own slice of [start, end)
                    tasks.Add(Task.Run(() =>
                    {
                        for (int path = start; path < end; path++)
                        {
                            double s = p.InitialPrice;

                            for (int step = 0; step < nSteps; step++)
                            {
                                double z = normals[path, step];
                                s *= Math.Exp(
                                    (p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt
                                    + p.Volatility * Math.Sqrt(dt) * z
                                );
                            }

                            terminals[path] = s;
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());

            }
            else
            {
                // --- Single-threaded execution ---
                for (int path = 0; path < nPaths; path++)
                {
                    double s = p.InitialPrice;
                    int step = 0;

                    while (step < nSteps)
                    {
                        var (z1, z2) = RandomNumberGenerator.NextTwoStandardNormals();

                        s *= Math.Exp((p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt
                                      + p.Volatility * Math.Sqrt(dt) * z1);
                        step++;

                        if (step < nSteps)
                        {
                            s *= Math.Exp((p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt
                                          + p.Volatility * Math.Sqrt(dt) * z2);
                            step++;
                        }
                    }

                    terminals[path] = s;
                }
            }

            // Convert to List<double> once at the end
            return new List<double>(terminals);
        }






        /// <summary> 
        /// Simulates terminal asset prices using antithetic variates. 
        /// Returns raw terminal prices (both halves) in sequence; collapsing is handled outside.
        /// </summary>

        private static List<double> SimulateAntitheticPrices(PricingParameters p)
        {
            int nPaths = p.NumberOfPaths;     // base paths (each yields s⁺ and s⁻)
            int nSteps = p.TimeSteps;
            double dt = p.TimeToExpiry / nSteps;
            double drift = (p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt;

            // Pre-generate all random numbers in a single thread
            // Each path × step will have one z; we’ll also use -z for the antithetic path
            double[,] normals = new double[nPaths, nSteps];
            for (int i = 0; i < nPaths; i++)
            {
                for (int j = 0; j < nSteps; j++)
                {
                    normals[i, j] = RandomNumberGenerator.NextStandardNormal();
                }
            }

            // Prepare storage for both terminal prices (s⁺ and s⁻ per path)
            var terminals = new double[2 * nPaths];

            if (p.UseMultithreading)
            {
                Console.WriteLine(" ");
                Console.WriteLine($"[INFO] Running multithreaded antithetic simulation on {Environment.ProcessorCount} cores...");

                int numThreads = Math.Max(1, Math.Min(Environment.ProcessorCount, nPaths));
                int baseCount = nPaths / numThreads;
                int remainder = nPaths % numThreads;

                var tasks = new List<Task>(numThreads);
                int offset = 0;

                for (int t = 0; t < numThreads; t++)
                {
                    int count = baseCount + (t < remainder ? 1 : 0);
                    int start = offset;
                    int end = start + count;
                    offset = end;

                    tasks.Add(Task.Run(() =>
                    {
                        for (int path = start; path < end; path++)
                        {
                            double sPlus = p.InitialPrice;
                            double sMinus = p.InitialPrice;

                            for (int step = 0; step < nSteps; step++)
                            {
                                double z = normals[path, step];
                                double diffusion = p.Volatility * Math.Sqrt(dt) * z;

                                // z → affects sPlus; -z → affects sMinus
                                sPlus *= Math.Exp(drift + diffusion);
                                sMinus *= Math.Exp(drift - diffusion);
                            }

                            // Thread-safe slice writes
                            terminals[2 * path] = sPlus;
                            terminals[2 * path + 1] = sMinus;
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                Console.WriteLine("[INFO] Running single-threaded antithetic simulation...");

                // --- Single-threaded execution ---
                for (int i = 0; i < nPaths; i++)
                {
                    double sPlus = p.InitialPrice;
                    double sMinus = p.InitialPrice;

                    for (int step = 0; step < nSteps; step++)
                    {
                        double z = RandomNumberGenerator.NextStandardNormal();
                        double diffusion = p.Volatility * Math.Sqrt(dt) * z;

                        sPlus *= Math.Exp(drift + diffusion);
                        sMinus *= Math.Exp(drift - diffusion);
                    }

                    terminals[2 * i] = sPlus;
                    terminals[2 * i + 1] = sMinus;
                }
            }

            // Convert to List<double> once at the end
            return new List<double>(terminals);
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




        public static (List<double> terminalPrices, List<double> pnlHedges) SimulateControlVariateTerminals(PricingParameters p)
        {
            int nPaths = p.NumberOfPaths;
            int nSteps = p.TimeSteps;
            double dt = p.TimeToExpiry / nSteps;
            double drift = (p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt;
            double volSqrtDt = p.Volatility * Math.Sqrt(dt);
            double discount = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);

            // 1) Pre-generate all random numbers in a single thread (thread-safe, reproducible)
            double[,] normals = new double[nPaths, nSteps];
            for (int i = 0; i < nPaths; i++)
                for (int j = 0; j < nSteps; j++)
                    normals[i, j] = RandomNumberGenerator.NextStandardNormal();

            // 2) Pre-allocate result arrays (filled in-place by slices)
            var terminalArr = new double[nPaths];
            var pnlHedgeArr = new double[nPaths];

            if (p.UseMultithreading)
            {
                Console.WriteLine();
                Console.WriteLine($"[INFO] Running multithreaded control-variate simulation on {Environment.ProcessorCount} cores...");

                int threads = Math.Max(1, Math.Min(Environment.ProcessorCount, nPaths));
                int baseCount = nPaths / threads;
                int remainder = nPaths % threads;

                var tasks = new List<Task>(threads);
                int offset = 0;

                for (int t = 0; t < threads; t++)
                {
                    int count = baseCount + (t < remainder ? 1 : 0);
                    int start = offset;
                    int end = start + count;
                    offset = end;

                    tasks.Add(Task.Run(() =>
                    {
                        for (int path = start; path < end; path++)
                        {
                            double s = p.InitialPrice;
                            double tCur = 0.0;

                            // Hedge init
                            double deltaPrev = BlackScholesDelta(s, tCur, p);
                            double cash = -deltaPrev * s;   // short delta shares; proceeds held as "cash"
                                                            // (keeping your original convention — no accrual here)
                                                            // Walk forward
                            for (int step = 0; step < nSteps; step++)
                            {
                                double z = normals[path, step];
                                s *= Math.Exp(drift + volSqrtDt * z);

                                tCur = (step + 1) * dt;
                                double delta = BlackScholesDelta(s, tCur, p);

                                // Rebalance hedge (self-financing: adjust cash by trade)
                                double dDelta = delta - deltaPrev;
                                cash -= dDelta * s;

                                deltaPrev = delta;
                            }

                            // Unwind hedge at expiry
                            cash += deltaPrev * s;

                            terminalArr[path] = s;
                            pnlHedgeArr[path] = discount * cash; // keep your "discount at end" convention
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                Console.WriteLine("[INFO] Running single-threaded control-variate simulation...");

                for (int path = 0; path < nPaths; path++)
                {
                    double s = p.InitialPrice;
                    double tCur = 0.0;

                    double deltaPrev = BlackScholesDelta(s, tCur, p);
                    double cash = -deltaPrev * s;

                    for (int step = 0; step < nSteps; step++)
                    {
                        double z = normals[path, step];
                        s *= Math.Exp(drift + volSqrtDt * z);

                        tCur = (step + 1) * dt;
                        double delta = BlackScholesDelta(s, tCur, p);

                        double dDelta = delta - deltaPrev;
                        cash -= dDelta * s;

                        deltaPrev = delta;
                    }

                    cash += deltaPrev * s;

                    terminalArr[path] = s;
                    pnlHedgeArr[path] = discount * cash;
                }
            }

            // 3) Convert once at the end
            return (new List<double>(terminalArr), new List<double>(pnlHedgeArr));
        }


        /// <summary>
        /// Utilizes antithetic sampling and control variate techniques to minimize variance
        /// <sumary>
        public static (List<double> terminalPrices, List<double> pnlHedges) SimulateAntitheticControlVariateTerminals(PricingParameters p)
        {
            int nPaths = p.NumberOfPaths;          // base paths; we’ll produce 2 results per base path
            int nSteps = p.TimeSteps;
            double dt = p.TimeToExpiry / nSteps;
            double driftStep = (p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt;
            double volSqrtDt = p.Volatility * Math.Sqrt(dt);
            double discount = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);

            // 1) Pre-generate all required standard normals (thread-safe & reproducible)
            double[,] normals = new double[nPaths, nSteps];
            for (int i = 0; i < nPaths; i++)
                for (int j = 0; j < nSteps; j++)
                    normals[i, j] = RandomNumberGenerator.NextStandardNormal();

            // 2) Pre-allocate outputs (2 per base path: plus and minus)
            var terminalArr = new double[2 * nPaths];
            var pnlHedgeArr = new double[2 * nPaths];

            if (p.UseMultithreading)
            {
                Console.WriteLine();
                Console.WriteLine($"[INFO] Running multithreaded antithetic CV simulation on {Environment.ProcessorCount} cores...");

                int threads = Math.Max(1, Math.Min(Environment.ProcessorCount, nPaths));
                int baseCount = nPaths / threads;
                int remainder = nPaths % threads;

                var tasks = new List<Task>(threads);
                int offset = 0;

                for (int t = 0; t < threads; t++)
                {
                    int count = baseCount + (t < remainder ? 1 : 0);
                    int start = offset;
                    int end = start + count;
                    offset = end;

                    tasks.Add(Task.Run(() =>
                    {
                        for (int path = start; path < end; path++)
                        {
                            // Two coupled (antithetic) paths
                            double sPlus = p.InitialPrice;
                            double sMinus = p.InitialPrice;

                            // Hedge states for each path
                            double tCur = 0.0;
                            double deltaPrevPlus = BlackScholesDelta(sPlus, tCur, p);
                            double deltaPrevMinus = BlackScholesDelta(sMinus, tCur, p);

                            double cashPlus = -deltaPrevPlus * sPlus;
                            double cashMinus = -deltaPrevMinus * sMinus;

                            for (int step = 0; step < nSteps; step++)
                            {
                                double z = normals[path, step];
                                double diff = volSqrtDt * z;

                                // Antithetic shocks: +z for sPlus, -z for sMinus
                                sPlus *= Math.Exp(driftStep + diff);
                                sMinus *= Math.Exp(driftStep - diff);

                                tCur = (step + 1) * dt;

                                double deltaPlus = BlackScholesDelta(sPlus, tCur, p);
                                double deltaMinus = BlackScholesDelta(sMinus, tCur, p);

                                // Rebalance self-financing hedge for each path
                                double dDeltaPlus = deltaPlus - deltaPrevPlus;
                                double dDeltaMinus = deltaMinus - deltaPrevMinus;

                                cashPlus -= dDeltaPlus * sPlus;
                                cashMinus -= dDeltaMinus * sMinus;

                                deltaPrevPlus = deltaPlus;
                                deltaPrevMinus = deltaMinus;
                            }

                            // Unwind the hedge at expiry for each path
                            cashPlus += deltaPrevPlus * sPlus;
                            cashMinus += deltaPrevMinus * sMinus;

                            terminalArr[2 * path] = sPlus;
                            terminalArr[2 * path + 1] = sMinus;

                            pnlHedgeArr[2 * path] = discount * cashPlus;
                            pnlHedgeArr[2 * path + 1] = discount * cashMinus;
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                Console.WriteLine("[INFO] Running single-threaded antithetic CV simulation...");

                for (int path = 0; path < nPaths; path++)
                {
                    double sPlus = p.InitialPrice;
                    double sMinus = p.InitialPrice;

                    double tCur = 0.0;
                    double deltaPrevPlus = BlackScholesDelta(sPlus, tCur, p);
                    double deltaPrevMinus = BlackScholesDelta(sMinus, tCur, p);

                    double cashPlus = -deltaPrevPlus * sPlus;
                    double cashMinus = -deltaPrevMinus * sMinus;

                    for (int step = 0; step < nSteps; step++)
                    {
                        double z = RandomNumberGenerator.NextStandardNormal();
                        double diff = volSqrtDt * z;

                        sPlus *= Math.Exp(driftStep + diff);
                        sMinus *= Math.Exp(driftStep - diff);

                        tCur = (step + 1) * dt;

                        double deltaPlus = BlackScholesDelta(sPlus, tCur, p);
                        double deltaMinus = BlackScholesDelta(sMinus, tCur, p);

                        double dDeltaPlus = deltaPlus - deltaPrevPlus;
                        double dDeltaMinus = deltaMinus - deltaPrevMinus;

                        cashPlus -= dDeltaPlus * sPlus;
                        cashMinus -= dDeltaMinus * sMinus;

                        deltaPrevPlus = deltaPlus;
                        deltaPrevMinus = deltaMinus;
                    }

                    cashPlus += deltaPrevPlus * sPlus;
                    cashMinus += deltaPrevMinus * sMinus;

                    terminalArr[2 * path] = sPlus;
                    terminalArr[2 * path + 1] = sMinus;

                    pnlHedgeArr[2 * path] = discount * cashPlus;
                    pnlHedgeArr[2 * path + 1] = discount * cashMinus;
                }
            }

            return (new List<double>(terminalArr), new List<double>(pnlHedgeArr));
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