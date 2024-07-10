namespace WebApiPrototype.Services
{
    /// <summary>
    /// Custom class registered in Program.cs for a long-running background service.
    /// </summary>
    public class MyPeriodicBackgroundService : BackgroundService
    {
        private readonly TimeSpan _period = TimeSpan.FromSeconds(5);
        private readonly ILogger<MyPeriodicBackgroundService> _logger;
        public MyPeriodicBackgroundService(ILogger<MyPeriodicBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation($"Executing MyPeriodicBackgroundService now at {DateTime.Now.ToLongTimeString()}");
            }
        }
    }
}
