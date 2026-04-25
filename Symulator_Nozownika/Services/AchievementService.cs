using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly AppDbContext _context;

        public AchievementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Achievement>> CheckTotalScoreAchievementsAsync(int userId, int currentTotalScore)
        {
            //list for storing newly unlocked achievements to return to the controller
            var newlyUnlocked = new List<Achievement>();

            //unlock achievements based on total score
            var unlockedAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserAccountId == userId && ua.Achievement.Type == AchievementType.TotalScore)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            //find achievements that are not yet unlocked and have target value less than or equal to current total score
            var achievementsToUnlock = await _context.Achievements
                .Where(a => a.Type == AchievementType.TotalScore
                         && !unlockedAchievementIds.Contains(a.Id)
                         && a.TargetValue <= currentTotalScore)
                .ToListAsync();

            if (!achievementsToUnlock.Any()) return newlyUnlocked;

            foreach (var achievement in achievementsToUnlock)
            {
                var newUnlock = new UserAchievement
                {
                    UserAccountId = userId,
                    AchievementId = achievement.Id,
                    UnlockedAt = DateTime.UtcNow
                };
                _context.UserAchievements.Add(newUnlock);

                // Add the newly unlocked achievement to the list to return to the controller
                newlyUnlocked.Add(achievement); 
            }

            await _context.SaveChangesAsync();
            //we return the list of newly unlocked achievements so that the controller can notify the user about them
            return newlyUnlocked; 
        }
    }
}
