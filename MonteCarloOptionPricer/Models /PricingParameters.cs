namespace MonteCarloOptionPricer.Models
{


    /// <summary>
    /// Defines which Monte Carlo algorithm to use.
    /// </summary>
    public enum SimulationMode
    {
        Plain,
        Antithetic,
        VanDerCorput,
        ControlVariate,
        Antithetic_and_ControlVariate
    }


    public class PricingParameters
    {
        public double InitialPrice { get; set; }
        public double Volatility { get; set; }
        public double RiskFreeRate { get; set; }
        public double TimeToExpiry { get; set; }  // in years
        public double Strike { get; set; }
        public bool isCall { get; set; }
        public int TimeSteps { get; set; }
        public int NumberOfPaths { get; set; }
        public bool UseMultithreading { get; set; } = false; // Default to false
        public SimulationMode SimMode { get; set; } = SimulationMode.Plain;
        
        // Parameters for Van der Corput (only used if SimMode == VanDerCorput)
        public int VdCBase1      { get; set; } = 2;
        public int VdCBase2      { get; set; } = 5;
        public int VdCPoints    { get; set; } = 1024;
    }

    /// <summary>
    /// Container for a Monte Carlo pricing result, including optional Greeks.
    /// </summary>
    public class PricingResult
    {
        /// <summary>Option price (present value).</summary>
        public double Price { get; set; }

        /// <summary>Approximate derivative dPrice/dSpot.</summary>
        public double? Delta { get; set; }

        /// <summary>Approximate second derivative d²Price/dSpot².</summary>
        public double? Gamma { get; set; }

        /// <summary>Sensitivity to volatility: dPrice/dVol.</summary>
        public double? Vega { get; set; }

        /// <summary>Sensitivity to time decay: -dPrice/dT.</summary>
        public double? Theta { get; set; }

        /// <summary>Sensitivity to interest rate: dPrice/dr.</summary>
        public double? Rho { get; set; }

        /// <summary>
        /// Standard error of the Monte Carlo price estimate.
        /// </summary>
        public double? StandardError { get; set; }
    }


}