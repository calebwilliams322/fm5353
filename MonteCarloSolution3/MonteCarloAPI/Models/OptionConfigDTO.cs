using System;

namespace MonteCarloAPI.Models
{
    // Combines both OptionParametersDTO and SimulationParametersDTO
    // into one configuration object used for creation, storage, and pricing.
    public class OptionConfigDTO
    {
        public int Id { get; set; }                               // Internal ID or reference
        public OptionParametersDTO OptionParameters { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    


}
