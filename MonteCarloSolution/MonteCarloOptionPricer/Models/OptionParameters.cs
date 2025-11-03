using System;

namespace MonteCarloOptionPricer.Models
{
    public class OptionParameters
    {
        // --- Core option characteristics ---
        public double Strike { get; set; } = 100.0;
        public bool IsCall { get; set; } = true;

        // --- Asian option fields ---
        public AveragingType AveragingType { get; set; } = AveragingType.Arithmetic;
        public int ObservationFrequency { get; set; } = 1;

        // --- Digital option fields ---
        public ConditionType DigitalCondition { get; set; } = ConditionType.AboveStrike;

        // --- Barrier option fields ---
        public BarrierType BarrierOptionType { get; set; } = BarrierType.KnockOut;
        public BarrierDirection BarrierDir { get; set; } = BarrierDirection.Up;
        public double BarrierLevel { get; set; } = 0.0;

        // --- Lookback option fields ---
        public LookbackType LookbackOptionType { get; set; } = LookbackType.Max;

        // --- Range option fields ---
        public int RangeObservationFrequency { get; set; } = 1;

        // --- Factory Methods ---
        
        /// <summary>
        /// Creates parameters for a European call option.
        /// </summary>
        public static OptionParameters CreateEuropeanCall(double strike)
        {
            return new OptionParameters
            {
                Strike = strike,
                IsCall = true
            };
        }

        /// <summary>
        /// Creates parameters for a European put option.
        /// </summary>
        public static OptionParameters CreateEuropeanPut(double strike)
        {
            return new OptionParameters
            {
                Strike = strike,
                IsCall = false
            };
        }

        /// <summary>
        /// Creates parameters for a barrier option.
        /// </summary>
        public static OptionParameters CreateBarrierOption(double strike, double barrierLevel, BarrierType barrierType, BarrierDirection barrierDir)
        {
            return new OptionParameters
            {
                Strike = strike,
                BarrierLevel = barrierLevel,
                BarrierOptionType = barrierType,
                BarrierDir = barrierDir
            };
        }

        /// <summary>
        /// Creates parameters for an Asian option.
        /// </summary>
        public static OptionParameters CreateAsianOption(double strike, AveragingType averagingType, int observationFrequency)
        {
            return new OptionParameters
            {
                Strike = strike,
                AveragingType = averagingType,
                ObservationFrequency = observationFrequency
            };
        }

        /// <summary>
        /// Creates parameters for a digital option.
        /// </summary>
        public static OptionParameters CreateDigitalOption(double strike, ConditionType conditionType)
        {
            return new OptionParameters
            {
                Strike = strike,
                DigitalCondition = conditionType
            };
        }

        /// <summary>
        /// Creates parameters for a lookback option.
        /// </summary>
        public static OptionParameters CreateLookbackOption(double strike, LookbackType lookbackType)
        {
            return new OptionParameters
            {
                Strike = strike,
                LookbackOptionType = lookbackType
            };
        }
    }

    // === Supporting enums for specific option behaviors ===

    public enum AveragingType { Arithmetic, Geometric }

    public enum ConditionType { AboveStrike, BelowStrike }

    public enum BarrierType { KnockIn, KnockOut }

    public enum BarrierDirection { Up, Down }

    public enum LookbackType { Max, Min }
}
