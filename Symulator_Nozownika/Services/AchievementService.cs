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

        public async Task CheckTotalScoreAchievementsAsync(int userId, int currentTotalScore)
        {
            //check if user has already unlocked achievements for total score
            var unlockedAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserAccountId == userId && ua.Achievement.Type == AchievementType.TotalScore)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            //check which achievements are still locked and should be unlocked based on current total score
            var achievementsToUnlock = await _context.Achievements
                .Where(a => a.Type == AchievementType.TotalScore
                         && !unlockedAchievementIds.Contains(a.Id)
                         && a.TargetValue <= currentTotalScore)
                .ToListAsync();

            //check if there are any achievements to unlock
            if (!achievementsToUnlock.Any()) return;

            //save new unlocked achievements to database
            foreach (var achievement in achievementsToUnlock)
            {
                var newUnlock = new UserAchievement
                {
                    UserAccountId = userId,
                    AchievementId = achievement.Id,
                    UnlockedAt = DateTime.UtcNow
                };
                _context.UserAchievements.Add(newUnlock);

                //for future: consider adding some notification logic here to inform user about new unlocked achievement
            }

            await _context.SaveChangesAsync();
        }
    }
}
