using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public class QuestService : IQuestService
    {
        private readonly AppDbContext _context;

        public QuestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserQuestProgress>> GetTodayQuestsAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var allQuests = await _context.Quests.ToListAsync();

            var progresses = await _context.UserQuestProgresses
                .Include(p => p.Quest)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var result = new List<UserQuestProgress>();

            foreach (var quest in allQuests)
            {
                var progress = progresses.FirstOrDefault(p => p.QuestId == quest.Id);

                if (progress == null)
                {
                    // Pierwszy kontakt z tym questem
                    progress = new UserQuestProgress
                    {
                        UserId = userId,
                        QuestId = quest.Id,
                        Quest = quest,
                        CurrentValue = 0,
                        IsCompleted = false,
                        IsRewardClaimed = false,
                        ProgressDate = today,
                        UpdatedAt = DateTime.Now
                    };
                    _context.UserQuestProgresses.Add(progress);
                }
                else if (progress.ProgressDate < today)
                {
                    // Nowy dzień — reset postępu
                    progress.CurrentValue = 0;
                    progress.IsCompleted = false;
                    progress.IsRewardClaimed = false;
                    progress.ProgressDate = today;
                    progress.UpdatedAt = DateTime.Now;
                    progress.Quest = quest;
                }
                else
                {
                    progress.Quest = quest;
                }

                result.Add(progress);
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<List<Quest>> UpdateProgressAsync(int userId, int gameScore, int gameClicks)
        {
            var today = DateTime.UtcNow.Date;
            var newlyCompleted = new List<Quest>();

            var progresses = await _context.UserQuestProgresses
                .Include(p => p.Quest)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var allQuests = await _context.Quests.ToListAsync();

            foreach (var quest in allQuests)
            {
                var progress = progresses.FirstOrDefault(p => p.QuestId == quest.Id);

                if (progress == null)
                {
                    progress = new UserQuestProgress
                    {
                        UserId = userId,
                        QuestId = quest.Id,
                        Quest = quest,
                        CurrentValue = 0,
                        IsCompleted = false,
                        IsRewardClaimed = false,
                        ProgressDate = today,
                        UpdatedAt = DateTime.Now
                    };
                    _context.UserQuestProgresses.Add(progress);
                }
                else if (progress.ProgressDate < today)
                {
                    // Reset dnia
                    progress.CurrentValue = 0;
                    progress.IsCompleted = false;
                    progress.IsRewardClaimed = false;
                    progress.ProgressDate = today;
                    progress.Quest = quest;
                }

                // Pomijaj już ukończone
                if (progress.IsCompleted)
                    continue;

                var gainedValue = quest.QuestType switch
                {
                    QuestType.Clicks => gameClicks,
                    QuestType.Score => gameScore,
                    _ => 0
                };

                progress.CurrentValue = Math.Min(progress.CurrentValue + gainedValue, quest.TargetValue);
                progress.UpdatedAt = DateTime.Now;

                if (progress.CurrentValue >= quest.TargetValue)
                {
                    progress.IsCompleted = true;
                    newlyCompleted.Add(quest);
                }
            }

            await _context.SaveChangesAsync();
            return newlyCompleted;
        }

        public async Task<bool> ClaimRewardAsync(int userId, int questProgressId)
        {
            var progress = await _context.UserQuestProgresses
                .Include(p => p.Quest)
                .FirstOrDefaultAsync(p => p.Id == questProgressId && p.UserId == userId);

            if (progress == null || !progress.IsCompleted || progress.IsRewardClaimed)
                return false;

            var today = DateTime.UtcNow.Date;
            if (progress.ProgressDate < today)
                return false; // quest z poprzedniego dnia

            // Przyznaj monety
            var wallet = await _context.CoinWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                wallet = new CoinWallet { UserId = userId, Balance = 0, UpdatedAt = DateTime.Now };
                _context.CoinWallets.Add(wallet);
            }
            wallet.Balance += progress.Quest.RewardCoins;
            wallet.UpdatedAt = DateTime.Now;

            // Przyznaj XP (dodaj do TotalScore w statystykach)
            var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
            if (stats != null)
            {
                stats.TotalScore += progress.Quest.RewardXp;
            }

            progress.IsRewardClaimed = true;
            progress.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
