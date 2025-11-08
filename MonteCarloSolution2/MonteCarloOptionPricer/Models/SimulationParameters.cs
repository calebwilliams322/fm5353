using System;

namespace MonteCarloOptionPricer.Models
{
    public class SimulationParameters
    {
        // --- Core GBM inputs ---
        public double InitialPrice { get; set; } = 100.0;
        public double Volatility { get; set; } = 0.2;
        public double RiskFreeRate { get; set; } = 0.05;
        public double TimeToExpiry { get; set; } = 1.0;  // in years

        // --- Discretization ---
        public int TimeSteps { get; set; } = 100;
        public int NumberOfPaths { get; set; } = 10000;

        // --- Execution settings ---
        public bool UseMultithreading { get; set; } = false;
        public SimulationMode SimMode { get; set; } = SimulationMode.Plain;

        public double ReferenceStrike { get; set; } = 100.0;

        // --- Quasi-random sequence settings (used only if SimMode == VanDerCorput) ---
        public int VdCBase1 { get; set; } = 2;
        public int VdCBase2 { get; set; } = 5;
        public int VdCPoints { get; set; } = 1024;

        // --- Factory Methods ---
        
        /// <summary>
        /// Creates default simulation parameters for plain Monte Carlo simulation.
        /// </summary>
        public static SimulationParameters CreateDefault()
        {
            return new SimulationParameters();
        }

        /// <summary>
        /// Creates simulation parameters for antithetic variates.
        /// </summary>
        public static SimulationParameters CreateAntithetic(int numberOfPaths, int timeSteps)
        {
            return new SimulationParameters
            {
                SimMode = SimulationMode.Antithetic,
                NumberOfPaths = numberOfPaths,
                TimeSteps = timeSteps
            };
        }

        /// <summary>
        /// Creates simulation parameters for quasi-random Van der Corput sequences.
        /// </summary>
        public static SimulationParameters CreateVanDerCorput(int vdCBase1, int vdCBase2, int vdCPoints)
        {
            return new SimulationParameters
            {
                SimMode = SimulationMode.VanDerCorput,
                VdCBase1 = vdCBase1,
                VdCBase2 = vdCBase2,
                VdCPoints = vdCPoints
            };
        }

        /// <summary>
        /// Creates simulation parameters for control variate simulation.
        /// </summary>
        public static SimulationParameters CreateControlVariate(int numberOfPaths, int timeSteps)
        {
            return new SimulationParameters
            {
                SimMode = SimulationMode.ControlVariate,
                NumberOfPaths = numberOfPaths,
                TimeSteps = timeSteps
            };
        }

        /// <summary>
        /// Creates simulation parameters for antithetic variates combined with control variates.
        /// </summary>
        public static SimulationParameters CreateAntitheticAndControlVariate(int numberOfPaths, int timeSteps)
        {
            return new SimulationParameters
            {
                SimMode = SimulationMode.AntitheticAndControlVariate,
                NumberOfPaths = numberOfPaths,
                TimeSteps = timeSteps
            };
        }
    }

    /// <summary>
    /// Enum representing the different simulation modes.
    /// </summary>
    public enum SimulationMode
    {
        Plain,
        Antithetic,
        ControlVariate,
        AntitheticAndControlVariate,
        VanDerCorput
    }
}