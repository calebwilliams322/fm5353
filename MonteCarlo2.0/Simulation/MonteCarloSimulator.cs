using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonteCarloOptionPricer.Models;

namespace MonteCarloOptionPricer.Simulation
{
    public static class MonteCarloSimulator
    {
        // === Entry Points ===
        public static SimulationOutput Simulate(SimulationParameters p, bool keepPaths = false)
        {
            return p.SimMode switch
            {
                SimulationMode.Plain => RunCore(p, keepPaths, useAntithetic: false, useVdC: false),
                SimulationMode.Antithetic => RunCore(p, keepPaths, useAntithetic: true, useVdC: false),
                SimulationMode.VanDerCorput => RunCore(p, keepPaths, useAntithetic: false, useVdC: true),
                SimulationMode.ControlVariate => RunControlVariate(p, keepPaths, useAntithetic: false),
                SimulationMode.AntitheticAndControlVariate => RunControlVariate(p, keepPaths, useAntithetic: true),
                _ => throw new ArgumentOutOfRangeException(nameof(p.SimMode), p.SimMode, "Unsupported mode")
            };
        }

        public static List<double> SimulateTerminals(SimulationParameters p)
            => Simulate(p).Terminals;

        public static List<double[]> SimulatePaths(SimulationParameters p)
            => Simulate(p, keepPaths: true).Paths!;

        // === Plain / Antithetic / VdC Core ===
        private static SimulationOutput RunCore(SimulationParameters p, bool keepPaths, bool useAntithetic, bool useVdC)
        {
            int basePaths = p.NumberOfPaths;
            int outCount = useAntithetic ? 2 * basePaths : basePaths;
            int steps = p.TimeSteps;

            double dt = p.TimeToExpiry / steps;
            double drift = (p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt;
            double volStep = p.Volatility * Math.Sqrt(dt);

            var terminals = new double[outCount];
            var paths = keepPaths ? new List<double[]>(outCount) : null;

            double[,]? normals = null;
            if (!useVdC)
            {
                normals = new double[basePaths, steps];
                for (int i = 0; i < basePaths; i++)
                    for (int j = 0; j < steps; j++)
                        normals[i, j] = RandomNumberGenerator.NextStandardNormal();
            }

            Parallel.For(0, basePaths, i =>
            {
                double sPlus = p.InitialPrice;
                double sMinus = p.InitialPrice;
                double[]? pathPlus = keepPaths ? new double[steps + 1] : null;
                double[]? pathMinus = (useAntithetic && keepPaths) ? new double[steps + 1] : null;
                if (keepPaths)
                {
                    pathPlus![0] = sPlus;
                    if (useAntithetic) pathMinus![0] = sMinus;
                }

                for (int j = 0; j < steps; j++)
                {
                    double z = useVdC ? BoxMullerFromVdC(i, j, p.VdCBase1, p.VdCBase2, steps)
                                      : normals![i, j];
                    sPlus *= Math.Exp(drift + volStep * z);
                    if (keepPaths) pathPlus![j + 1] = sPlus;

                    if (useAntithetic)
                    {
                        sMinus *= Math.Exp(drift - volStep * z);
                        if (keepPaths) pathMinus![j + 1] = sMinus;
                    }
                }

                if (useAntithetic)
                {
                    terminals[2 * i] = sPlus;
                    terminals[2 * i + 1] = sMinus;
                    if (keepPaths)
                    {
                        lock (paths!) { paths.Add(pathPlus!); paths.Add(pathMinus!); }
                    }
                }
                else
                {
                    terminals[i] = sPlus;
                    if (keepPaths) lock (paths!) { paths.Add(pathPlus!); }
                }
            });

            return new SimulationOutput(new List<double>(terminals), paths, hedgePnL: null);
        }

        // === Control Variate Core ===
        private static SimulationOutput RunControlVariate(SimulationParameters p, bool keepPaths, bool useAntithetic)
        {
            int basePaths = p.NumberOfPaths;
            int outCount = useAntithetic ? 2 * basePaths : basePaths;
            int steps = p.TimeSteps;

            double dt = p.TimeToExpiry / steps;
            double drift = (p.RiskFreeRate - 0.5 * p.Volatility * p.Volatility) * dt;
            double volStep = p.Volatility * Math.Sqrt(dt);
            double discount = Math.Exp(-p.RiskFreeRate * p.TimeToExpiry);

            var terminals = new double[outCount];
            var hedgePnL = new double[outCount];
            var paths = keepPaths ? new List<double[]>(outCount) : null;

            double[,] normals = new double[basePaths, steps];
            for (int i = 0; i < basePaths; i++)
                for (int j = 0; j < steps; j++)
                    normals[i, j] = RandomNumberGenerator.NextStandardNormal();

            Parallel.For(0, basePaths, i =>
            {
                double sPlus = p.InitialPrice;
                double sMinus = p.InitialPrice;
                double tCur = 0.0;

                double deltaPrevPlus = BlackScholesDelta(sPlus, tCur, p);
                double deltaPrevMinus = useAntithetic ? BlackScholesDelta(sMinus, tCur, p) : 0.0;
                double cashPlus = -deltaPrevPlus * sPlus;
                double cashMinus = useAntithetic ? -deltaPrevMinus * sMinus : 0.0;

                double[]? pathPlus = keepPaths ? new double[steps + 1] : null;
                double[]? pathMinus = (useAntithetic && keepPaths) ? new double[steps + 1] : null;
                if (keepPaths)
                {
                    pathPlus![0] = sPlus;
                    if (useAntithetic) pathMinus![0] = sMinus;
                }

                for (int j = 0; j < steps; j++)
                {
                    double z = normals[i, j];
                    double diff = volStep * z;
                    sPlus *= Math.Exp(drift + diff);
                    if (keepPaths) pathPlus![j + 1] = sPlus;

                    tCur = (j + 1) * dt;
                    double deltaPlus = BlackScholesDelta(sPlus, tCur, p);
                    cashPlus -= (deltaPlus - deltaPrevPlus) * sPlus;
                    deltaPrevPlus = deltaPlus;

                    if (useAntithetic)
                    {
                        sMinus *= Math.Exp(drift - diff);
                        if (keepPaths) pathMinus![j + 1] = sMinus;

                        double deltaMinus = BlackScholesDelta(sMinus, tCur, p);
                        cashMinus -= (deltaMinus - deltaPrevMinus) * sMinus;
                        deltaPrevMinus = deltaMinus;
                    }
                }

                cashPlus += deltaPrevPlus * sPlus;
                if (useAntithetic) cashMinus += deltaPrevMinus * sMinus;

                terminals[useAntithetic ? 2 * i : i] = sPlus;
                hedgePnL[useAntithetic ? 2 * i : i] = discount * cashPlus;

                if (useAntithetic)
                {
                    terminals[2 * i + 1] = sMinus;
                    hedgePnL[2 * i + 1] = discount * cashMinus;
                }

                if (keepPaths)
                {
                    lock (paths!)
                    {
                        paths.Add(pathPlus!);
                        if (useAntithetic) paths.Add(pathMinus!);
                    }
                }
            });

            return new SimulationOutput(new List<double>(terminals), paths, new List<double>(hedgePnL));
        }

        // === Helper Math ===
        private static double BoxMullerFromVdC(int i, int j, int b1, int b2, int steps)
        {
            int k = i * steps + j + 1;
            double u = VanDerCorput(k, b1);
            double v = VanDerCorput(k, b2);
            if (u <= double.Epsilon) u = double.Epsilon;
            double R = Math.Sqrt(-2.0 * Math.Log(u));
            double theta = 2.0 * Math.PI * v;
            return R * Math.Cos(theta);
        }

        private static double VanDerCorput(int n, int baseNum)
        {
            double q = 0.0, bk = 1.0 / baseNum;
            while (n > 0)
            {
                q += (n % baseNum) * bk;
                n /= baseNum;
                bk /= baseNum;
            }
            return q;
        }

        private static double NormalCdf(double x)
        {
            if (x < 0) return 1.0 - NormalCdf(-x);
            double p = 0.2316419;
            double b1 = 0.319381530, b2 = -0.356563782, b3 = 1.781477937,
                   b4 = -1.821255978, b5 = 1.330274429;
            double t = 1.0 / (1.0 + p * x);
            double pdf = Math.Exp(-0.5 * x * x) / Math.Sqrt(2.0 * Math.PI);
            double poly = (((((b5 * t + b4) * t) + b3) * t + b2) * t + b1) * t;
            return 1.0 - pdf * poly;
        }

        private static double BlackScholesDelta(double S, double t, SimulationParameters p)
        {
            double T = p.TimeToExpiry;
            double tau = T - t;
            if (tau <= 1e-12) return 0.0;

            double K = p.ReferenceStrike;
            double r = p.RiskFreeRate;
            double sigma = p.Volatility;
            double denom = sigma * Math.Sqrt(tau);
            double d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * tau) / denom;
            return NormalCdf(d1);
        }

        // === Output Container ===
        public sealed class SimulationOutput
        {
            public List<double> Terminals { get; }
            public List<double[]>? Paths { get; }
            public List<double>? HedgePnL { get; }

            public SimulationOutput(List<double> terminals, List<double[]>? paths, List<double>? hedgePnL)
            {
                Terminals = terminals;
                Paths = paths;
                HedgePnL = hedgePnL;
            }
        }
    }
}