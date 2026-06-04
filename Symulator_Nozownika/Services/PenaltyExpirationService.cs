namespace Symulator_Nozownika.Services
{
    public interface IPenaltyExpirationService
    {
        Task CheckAndExpirePenaltiesAsync();
    }

    public class PenaltyExpirationService : IPenaltyExpirationService
    {
        private readonly IReportManagementService _reportService;
        private readonly ILogger<PenaltyExpirationService> _logger;

        public PenaltyExpirationService(IReportManagementService reportService, ILogger<PenaltyExpirationService> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        public async Task CheckAndExpirePenaltiesAsync()
        {
            try
            {
                await _reportService.CheckAndExpirePenaltiesAsync();
                _logger.LogInformation("Penalty expiration check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and expiring penalties");
            }
        }
    }
}
