namespace MonteCarloAPI.Models
{
    // Combined response containing both the option configuration and pricing results
    public class PricingResponseDTO
    {
        public OptionConfigDTO Option { get; set; } = new();
        public PricingResultDTO PricingResult { get; set; } = new();
        public SimulationParametersDTO SimulationParameters { get; set; } = new();
    }
}
