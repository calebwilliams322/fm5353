namespace MonteCarloAPIv3.DTOs
{
    public class PricingRequestDto
    {
        public OptionDto Option { get; set; } = new();
        public int TimeSteps { get; set; } = 252;
        public int NumberOfPaths { get; set; } = 10000;
        public string SimulationMode { get; set; } = "Plain";
        public bool CalculateGreeks { get; set; } = true;
        public bool UseMultithreading { get; set; } = true;
    }
}