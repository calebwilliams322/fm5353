namespace MonteCarloAPIv3.DTOs
{
    public class OptionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "European";
        public double InitialPrice { get; set; }
        public double Strike { get; set; }
        public double Volatility { get; set; }
        public double RiskFreeRate { get; set; }
        public double TimeToExpiry { get; set; }
        public bool IsCall { get; set; }
    }
}