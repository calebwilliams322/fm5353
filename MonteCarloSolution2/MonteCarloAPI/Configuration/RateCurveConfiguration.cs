namespace MonteCarloAPI.Configuration
{
    /// <summary>
    /// Configuration for the risk-free rate curve used in option pricing.
    /// Loaded from appsettings.json.
    /// </summary>
    public class RateCurveConfiguration
    {
        public List<RateCurvePoint> Points { get; set; } = new();
    }

    /// <summary>
    /// A single point on the rate curve (tenor, rate pair)
    /// </summary>
    public class RateCurvePoint
    {
        /// <summary>
        /// Time to maturity in years (e.g., 0.25 = 3 months, 1.0 = 1 year)
        /// </summary>
        public double TenorYears { get; set; }

        /// <summary>
        /// Risk-free rate at this tenor (e.g., 0.05 = 5%)
        /// </summary>
        public double Rate { get; set; }
    }
}
