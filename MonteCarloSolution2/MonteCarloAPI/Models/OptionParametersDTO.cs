namespace MonteCarloAPI.Models
{
    
    
    // DTO representing the financial contract parameters for an option.
    // Matches your backend OptionParameters class exactly (including enums).
    public class OptionParametersDTO
    {
        // --- Core parameters ---
        public OptionType OptionType { get; set; } = OptionType.European;
        public double Strike { get; set; } = 100.0;
        public bool IsCall { get; set; } = true;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1); // Default 1 year from now

        // --- Asian option parameters ---
        public AveragingType AveragingType { get; set; } = AveragingType.Arithmetic;
        public int ObservationFrequency { get; set; } = 1;

        // --- Digital option parameters ---
        public ConditionType DigitalCondition { get; set; } = ConditionType.AboveStrike;

        // --- Barrier option parameters ---
        public BarrierType BarrierOptionType { get; set; } = BarrierType.KnockOut;
        public BarrierDirection BarrierDir { get; set; } = BarrierDirection.Up;
        public double BarrierLevel { get; set; } = 0.0;

        // --- Lookback option parameters ---
        public LookbackType LookbackOptionType { get; set; } = LookbackType.Max;

        // --- Range option parameters ---
        public int RangeObservationFrequency { get; set; } = 1;
    }

    // --- Matching enums copied from backend ---

    public enum AveragingType
    {
        Arithmetic,
        Geometric
    }

    public enum ConditionType
    {
        AboveStrike,
        BelowStrike
    }

    public enum BarrierType
    {
        KnockIn,
        KnockOut
    }

    public enum BarrierDirection
    {
        Up,
        Down
    }

    public enum LookbackType
    {
        Max,
        Min
    }

    public enum OptionType
    {
        European = 0,
        Asian = 1,
        Digital = 2,
        Barrier = 3,
        Lookback = 4,
        Range = 5
    }
}
