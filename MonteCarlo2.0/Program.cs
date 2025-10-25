using System;
using MonteCarloOptionPricer.Models;
using MonteCarloOptionPricer.Simulation;

namespace MonteCarloOptionPricer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Monte Carlo Simulator Smoke Test ===\n");

            // --- Define shared parameters ---
            var simParams = new SimulationParameters
            {
                InitialPrice = 100.0,
                Volatility = 0.2,
                RiskFreeRate = 0.05,
                TimeToExpiry = 1.0,
                TimeSteps = 252,
                NumberOfPaths = 10000,
                SimMode = SimulationMode.Plain,
                UseMultithreading = false,
                ReferenceStrike = 100.0
            };

            // --- Plain GBM Test ---
            simParams.SimMode = SimulationMode.Plain;
            var plain = MonteCarloSimulator.Simulate(simParams);
            PrintSummary("Plain", plain);

            // --- Antithetic Test ---
            simParams.SimMode = SimulationMode.Antithetic;
            var anti = MonteCarloSimulator.Simulate(simParams);
            PrintSummary("Antithetic", anti);

            // --- Van der Corput Test ---
            simParams.SimMode = SimulationMode.VanDerCorput;
            var vdc = MonteCarloSimulator.Simulate(simParams);
            PrintSummary("Van der Corput", vdc);

            // --- Control Variate Test ---
            simParams.SimMode = SimulationMode.ControlVariate;
            var cv = MonteCarloSimulator.Simulate(simParams);
            PrintSummary("Control Variate", cv, includeHedge: true);

            // --- Antithetic + CV Test ---
            simParams.SimMode = SimulationMode.AntitheticAndControlVariate;
            var cvAnti = MonteCarloSimulator.Simulate(simParams);
            PrintSummary("Antithetic + Control Variate", cvAnti, includeHedge: true);

            Console.WriteLine("\n=== Tests Complete ===");
        }

        // --- Helper printer ---
        static void PrintSummary(string label, MonteCarloSimulator.SimulationOutput output, bool includeHedge = false)
        {
            var mean = Mean(output.Terminals);
            var std = Std(output.Terminals, mean);
            Console.WriteLine($"[{label}]");
            Console.WriteLine($"Paths: {output.Terminals.Count}");
            Console.WriteLine($"Mean terminal: {mean:F4}, StdDev: {std:F4}");

            if (includeHedge && output.HedgePnL != null)
            {
                var meanPnL = Mean(output.HedgePnL);
                var stdPnL = Std(output.HedgePnL, meanPnL);
                Console.WriteLine($"Mean hedge PnL: {meanPnL:F4}, StdDev: {stdPnL:F4}");
            }

            Console.WriteLine();
        }

        static double Mean(IReadOnlyList<double> x)
        {
            double sum = 0;
            foreach (var xi in x) sum += xi;
            return sum / x.Count;
        }

        static double Std(IReadOnlyList<double> x, double mean)
        {
            double sum = 0;
            foreach (var xi in x)
            {
                var diff = xi - mean;
                sum += diff * diff;
            }
            return Math.Sqrt(sum / (x.Count - 1));
        }
    }
}