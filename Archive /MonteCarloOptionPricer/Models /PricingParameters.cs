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
        public int VdCBase1 { get; set; } = 2;
        public int VdCBase2 { get; set; } = 5;
        public int VdCPoints { get; set; } = 1024;


        // Asian Option Additions

        public AveragingType AverageType { get; set; } = AveragingType.Arithmetic;
        public int ObservationFrequency { get; set; } = 1; // Number of steps between observations


        // Digital Option Additions 
        public ConditionType DigitalCondition { get; set; } = ConditionType.AboveStrike;



        // Barrier Option Additions 
        public BarrierType BarrierOptionType { get; set; } = BarrierType.KnockOut;
        public BarrierDirection BarrierDir { get; set; } = BarrierDirection.Up;
        public double BarrierLevel { get; set; }


        // Lookback Option Additions 
        public LookbackType LookbackOptionType { get; set; } = LookbackType.Max;


        // Range Option Additions 
        public int RangeObservationFrequency { get; set; } = 1; // Number of steps between observations
        



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



    // Additional Enums needed for new option types 
    // Will be included in the pricing parameters object even if not needed for certain options


    // Average type for asian options
    public enum AveragingType
    {
        Arithmetic,
        Geometric
    }


    // Condition type for digi payoff (simply above or below strike of option)
    public enum ConditionType
    {
        AboveStrike,
        BelowStrike
    }


    // Barrier type for Barrier Options 

    public enum BarrierType
    {
        KnockIn,
        KnockOut
    }


    // Barrier Direction
    public enum BarrierDirection
    {
        Up,
        Down
    }


    // Lookback Type 

    public enum LookbackType
    {
        Max,
        Min
    }











}