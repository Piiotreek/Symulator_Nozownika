using System.Threading.Tasks;

namespace Symulator_Nozownika.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateClubCsvAsync(int clubId);
        Task<byte[]> GenerateUserStatisticsCsvAsync(int userId);
        Task<byte[]> GenerateAllStatisticsCsvAsync();
        Task<byte[]> GenerateHighscoresCsvAsync();
    }
}
