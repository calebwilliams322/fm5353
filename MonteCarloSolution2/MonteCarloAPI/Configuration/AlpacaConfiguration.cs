namespace MonteCarloAPI.Configuration
{
    /// <summary>
    /// Configuration for Alpaca API integration
    /// </summary>
    public class AlpacaConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool UsePaperTrading { get; set; } = true;
        public int UpdateIntervalMinutes { get; set; } = 5;
    }
}
