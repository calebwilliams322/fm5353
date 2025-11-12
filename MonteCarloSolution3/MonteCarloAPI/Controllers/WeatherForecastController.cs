using Microsoft.AspNetCore.Mvc;
using MonteCarloAPI.Services; // <-- this is the key addition

namespace MonteCarloAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly Service1 _service; // <-- inject Service1

        public WeatherForecastController(ILogger<WeatherForecastController> logger, Service1 service)
        {
            _logger = logger;
            _service = service; // <-- assign it
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            // Normal weather data generation
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // NEW: call your service and log the result
            var message = _service.GetMessage();
            _logger.LogInformation($"Service1 message: {message}");

            return forecast;
        }
    }
}
