using PuppeteerSharp;

namespace Symulator_Nozownika.Services
{
    /// <summary>
    /// Singleton service managing a single shared Chromium browser instance.
    /// Prevents race conditions and file-lock errors caused by multiple concurrent
    /// BrowserFetcher.DownloadAsync() calls from separate scoped ReportService instances.
    /// </summary>
    public class BrowserService : IBrowserService
    {
        private IBrowser? _browser;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger<BrowserService> _logger;

        public BrowserService(ILogger<BrowserService> logger)
        {
            _logger = logger;
        }

        public async Task<IBrowser> GetBrowserAsync()
        {
            // Fast path: browser already running
            if (_browser != null && !_browser.IsClosed)
                return _browser;

            await _semaphore.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_browser != null && !_browser.IsClosed)
                    return _browser;

                _logger.LogInformation("Downloading/verifying Chromium installation...");
                var fetcher = new BrowserFetcher();
                await fetcher.DownloadAsync();

                _logger.LogInformation("Launching Chromium browser...");
                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-gpu"
                    }
                });

                _browser.Disconnected += (_, _) =>
                {
                    _logger.LogWarning("Chromium browser disconnected unexpectedly. It will be re-launched on next request.");
                    _browser = null;
                };

                return _browser;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null && !_browser.IsClosed)
            {
                await _browser.CloseAsync();
                _browser.Dispose();
                _browser = null;
            }
            _semaphore.Dispose();
        }
    }
}
