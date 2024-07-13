using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
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

        // concurrency solution for the caching example below
        private static readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);

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
        public async Task<IEnumerable<WeatherForecast>> GetAsync(IMemoryCache cache)
        {
            if (_options.Value.ReportErrors)
            {
                // so something with _options.Value.ErrorMessage
            }

            #region Caching Example
            int id = 7;
            if (!cache.TryGetValue(id, out WeatherForecast? forecast))
            {
                try
                {
                    // we're using a semaphore here to address concurrency and prevent a cache stampede
                    await _semaphore.WaitAsync();
                    forecast = new WeatherForecast();

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    cache.Set(id, forecast, cacheEntryOptions);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            // do something with the value now that it's either been
            // returned from the cache, or retrieved and then cached
            #endregion

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