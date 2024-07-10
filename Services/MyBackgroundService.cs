namespace WebApiPrototype.Services
{
    /// <summary>
    /// Custom class registered in Program.cs that will execute when the application host is ready to start and [graceful] stop.
    /// NOTE: StopAsync will NOT trigger when you're debugging unless you're using IIS and stop the site,
    /// or use the Stop Site in the Windows system tray for IIS Express
    /// </summary>
    public class MyBackgroundService : IHostedService
    {
        private readonly ILogger<MyBackgroundService> _logger;

        public MyBackgroundService(ILogger<MyBackgroundService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Service MyBackgroundService");
            await Task.Delay(5000);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop Service MyBackgroundService");
            await Task.Delay(1000);
        }
    }
}
