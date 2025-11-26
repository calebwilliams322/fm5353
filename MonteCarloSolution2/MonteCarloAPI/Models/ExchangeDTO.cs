using MonteCarloAPI.Data;

namespace MonteCarloAPI.Models
{
    /// <summary>
    /// DTO for Exchange information
    /// </summary>
    public class ExchangeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Currency { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Converts an ExchangeEntity to DTO
        /// </summary>
        public static ExchangeDTO FromEntity(ExchangeEntity entity)
        {
            return new ExchangeDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Country = entity.Country,
                Currency = entity.Currency,
                CreatedAt = entity.CreatedAt
            };
        }
    }

    /// <summary>
    /// DTO for creating a new exchange
    /// </summary>
    public class CreateExchangeDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Currency { get; set; }
    }
}
