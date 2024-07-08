using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<CustomOptions> _options;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFeatureManager featureManager, IOptions<CustomOptions> options)
        {
            _logger = logger;
            _featureManager = featureManager;
            _options = options;
        }

        [FeatureGate("MyEnableGetFeatureFlag")]
        //[FeatureGate("MyPercentageFeatureFlag")] // can also use a percentage-based feature gate
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            if (_options.Value.ReportErrors)
            {
                // so something with _options.Value.ErrorMessage
            }

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