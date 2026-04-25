using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public interface IAchievementService
    {
        Task<List<Achievement>> CheckTotalScoreAchievementsAsync(int userId, int currentTotalScore);
        //for future checking other types of achievements, like total games played, total clicks, etc.
    }
}
