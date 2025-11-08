namespace MonteCarloAPIv3.DTOs
{
    public class PricingResultDto
    {
        public double Price { get; set; }
        public double? StdError { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Vega { get; set; }
        public double Theta { get; set; }
        public double Rho { get; set; }
    }
}