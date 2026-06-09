using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public interface IAchievementService
    {
        Task<List<Achievement>> CheckTotalScoreAchievementsAsync(int userId, int currentTotalScore);
        Task<List<Achievement>> CheckTotalClicksAchievementsAsync(int userId, int currentTotalClicks);
        Task<List<Achievement>> CheckSingleGameClicksAchievementAsync(int userId, int singleGameClicks);
        Task<List<Achievement>> CheckFirstGameAchievementAsync(int userId, int totalGamesPlayed);
        Task<List<Achievement>> CheckFirstKillAchievementAsync(int userId, bool won);
    }
}
