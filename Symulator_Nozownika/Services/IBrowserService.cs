using PuppeteerSharp;

namespace Symulator_Nozownika.Services
{
    public interface IBrowserService : IAsyncDisposable
    {
        Task<IBrowser> GetBrowserAsync();
    }
}
