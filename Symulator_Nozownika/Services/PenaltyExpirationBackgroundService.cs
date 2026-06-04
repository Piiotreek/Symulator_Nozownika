namespace Symulator_Nozownika.Services
{
    public class PenaltyExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PenaltyExpirationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public PenaltyExpirationBackgroundService(IServiceProvider serviceProvider, ILogger<PenaltyExpirationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Penalty expiration background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var penaltyService = scope.ServiceProvider.GetRequiredService<IPenaltyExpirationService>();
                        await penaltyService.CheckAndExpirePenaltiesAsync();
                    }

                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in penalty expiration background service");
                    await Task.Delay(_checkInterval, stoppingToken);
                }
            }

            _logger.LogInformation("Penalty expiration background service stopped");
        }
    }
}
