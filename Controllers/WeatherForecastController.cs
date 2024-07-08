using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace WebApiPrototype.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableRateLimiting("FiveRequestsPerTenSeconds")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly IFeatureManager _featureManager;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFeatureManager featureManager)
        {
            _logger = logger;
            _featureManager = featureManager;
        }

        [FeatureGate("MyEnableGetFeatureFlag")]
        //[FeatureGate("MyPercentageFeatureFlag")] // can also use a percentage-based feature gate
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            int totalDaysToReturn = 5;
            if (await _featureManager.IsEnabledAsync("MyFeatureFlag"))
            {
                totalDaysToReturn = 10;
            }
            return Enumerable.Range(1, totalDaysToReturn).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}