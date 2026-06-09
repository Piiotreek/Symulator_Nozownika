using System.Threading.Tasks;

namespace Symulator_Nozownika.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateClubPdfAsync(int clubId);
        Task<byte[]> GenerateUserStatisticsPdfAsync(int userId);
        Task<byte[]> GenerateAllStatisticsPdfAsync();
        Task<byte[]> GenerateHighscoresPdfAsync();
    }
}
