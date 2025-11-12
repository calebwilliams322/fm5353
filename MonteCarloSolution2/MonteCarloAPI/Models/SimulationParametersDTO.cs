namespace MonteCarloAPI.Models
{
    // DTO representing simulation and market parameters.
    // Matches your backend SimulationParameters class exactly.
    public class SimulationParametersDTO
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

        // --- Quasi-random sequence settings ---
        public int VdCBase1 { get; set; } = 2;
        public int VdCBase2 { get; set; } = 5;
        public int VdCPoints { get; set; } = 1024;
    }

    // --- Matching enum from backend ---
    public enum SimulationMode
    {
        Plain,
        Antithetic,
        ControlVariate,
        AntitheticAndControlVariate,
        VanDerCorput
    }
}
