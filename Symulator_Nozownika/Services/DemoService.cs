using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public interface IDemoService
    {
        Task<UserAccount?> GetOrCreateDemoUserAsync();
        Task<bool> CanDemoUserPlayAsync();
        Task<(bool CanPlay, string Message)> CheckDemoStatusAsync();
        Task IncrementDemoGamesAndCheckLockAsync(UserAccount demoUser);
        Task<TimeSpan> GetTimeUntilUnlockAsync();
        Task<DemoLockout?> GetCurrentDemoLockoutAsync();
        Task CreateDemoUserAsync();
    }

    public class DemoService : IDemoService
    {
        private readonly AppDbContext _context;
        private const int DEMO_SESSION_HOURS = 6;
        private const int MAX_DEMO_GAMES = 3;
        private const string DEMO_USERNAME = "demo";
        private const int STARTER_WEAPON_ID = 2; // Scissors

        public DemoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserAccount?> GetOrCreateDemoUserAsync()
        {
            var demoUser = await _context.UserAccounts
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .Include(u => u.CoinWallet)
                .Include(u => u.SelectedWeapon)
                .FirstOrDefaultAsync(u => u.UserName == DEMO_USERNAME && u.IsDemo);

            if (demoUser != null)
            {
                var lockout = await GetCurrentDemoLockoutAsync();
                if (lockout?.IsLocked == true)
                {
                    return demoUser;
                }

                // Lockout expired, delete old demo user and create new one
                await DeleteDemoUserAsync(demoUser);
                return await CreateNewDemoUserAsync();
            }

            return await CreateNewDemoUserAsync();
        }

        public async Task CreateDemoUserAsync()
        {
            await CreateNewDemoUserAsync();
        }

        private async Task<UserAccount?> CreateNewDemoUserAsync()
        {
            // Check if lockout exists
            var existingLockout = await _context.DemoLockouts.FirstOrDefaultAsync();
            if (existingLockout?.IsLocked == true)
            {
                return null; // Demo is still locked
            }

            // Create lockout for next 6 hours
            var lockout = new DemoLockout
            {
                LockedUntil = DateTime.UtcNow.AddHours(DEMO_SESSION_HOURS),
                CreatedAt = DateTime.UtcNow
            };

            // Remove old lockout if exists
            var oldLockout = await _context.DemoLockouts.FirstOrDefaultAsync();
            if (oldLockout != null)
            {
                _context.DemoLockouts.Remove(oldLockout);
            }

            _context.DemoLockouts.Add(lockout);
            await _context.SaveChangesAsync();

            var starterWeapon = await _context.Weapons.FirstOrDefaultAsync(w => w.Id == STARTER_WEAPON_ID);

            var demoUser = new UserAccount
            {
                FirstName = "Demo",
                LastName = "Player",
                Email = $"demo_{Guid.NewGuid().ToString().Substring(0, 8)}@demo.local",
                Country = "Demo",
                UserName = DEMO_USERNAME,
                Password = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                IsDemo = true,
                DemoGamesPlayed = 0,
                SelectedWeaponId = starterWeapon?.Id
            };

            _context.UserAccounts.Add(demoUser);
            await _context.SaveChangesAsync();

            // Create wallet
            var wallet = new CoinWallet
            {
                UserId = demoUser.Id,
                Balance = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CoinWallets.Add(wallet);

            // Create statistics
            var stats = new UserStatistics
            {
                UserId = demoUser.Id,
                TotalGamesPlayed = 0,
                TotalScore = 0,
                HighestScore = 0,
                TotalClicks = 0,
                TotalPlayTime = TimeSpan.Zero,
                CurrentStreak = 0,
                LongestStreak = 0,
                LastPlayedAt = DateTime.UtcNow
            };
            _context.UserStatistics.Add(stats);

            // Create level
            var level = new Level
            {
                UserId = demoUser.Id,
                CurrentLevel = 1,
                CurrentLevelThreshold = 0,
                NextLevelThreshold = 100,
                TotalScoreSnapshot = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Levels.Add(level);

            await _context.SaveChangesAsync();

            return demoUser;
        }

        private async Task DeleteDemoUserAsync(UserAccount demoUser)
        {
            // Delete related data
            var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == demoUser.Id);
            if (stats != null)
            {
                _context.UserStatistics.Remove(stats);
            }

            var level = await _context.Levels.FirstOrDefaultAsync(l => l.UserId == demoUser.Id);
            if (level != null)
            {
                _context.Levels.Remove(level);
            }

            var wallet = await _context.CoinWallets.FirstOrDefaultAsync(w => w.UserId == demoUser.Id);
            if (wallet != null)
            {
                _context.CoinWallets.Remove(wallet);
            }

            var achievements = await _context.UserAchievements
                .Where(ua => ua.UserAccountId == demoUser.Id)
                .ToListAsync();
            if (achievements.Any())
            {
                _context.UserAchievements.RemoveRange(achievements);
            }

            var questProgress = await _context.UserQuestProgresses
                .Where(uq => uq.UserId == demoUser.Id)
                .ToListAsync();
            if (questProgress.Any())
            {
                _context.UserQuestProgresses.RemoveRange(questProgress);
            }

            _context.UserAccounts.Remove(demoUser);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanDemoUserPlayAsync()
        {
            var demoUser = await GetOrCreateDemoUserAsync();
            if (demoUser == null)
                return false;

            return demoUser.DemoGamesPlayed < MAX_DEMO_GAMES;
        }

        public async Task<(bool CanPlay, string Message)> CheckDemoStatusAsync()
        {
            var demoUser = await GetOrCreateDemoUserAsync();

            if (demoUser == null)
            {
                var lockout = await GetCurrentDemoLockoutAsync();
                if (lockout?.IsLocked == true)
                {
                    var timeRemaining = lockout.LockedUntil - DateTime.UtcNow;
                    return (false, $"🔒 Demo mode is locked! You can play in {timeRemaining.Hours}h {timeRemaining.Minutes}m");
                }

                return (false, "Unable to create demo user.");
            }

            if (demoUser.DemoGamesPlayed >= MAX_DEMO_GAMES)
            {
                return (false, $"📊 You've played {MAX_DEMO_GAMES} demo games today. Come back in 6 hours!");
            }

            var remaining = MAX_DEMO_GAMES - demoUser.DemoGamesPlayed;
            return (true, $"✅ You can play! Games remaining: {remaining}");
        }

        public async Task IncrementDemoGamesAndCheckLockAsync(UserAccount demoUser)
        {
            if (demoUser == null)
                return;

            demoUser.DemoGamesPlayed++;
            _context.UserAccounts.Update(demoUser);
            await _context.SaveChangesAsync();

            // If reached limit, mark for deletion and create new lockout
            if (demoUser.DemoGamesPlayed >= MAX_DEMO_GAMES)
            {
                await DeleteDemoUserAsync(demoUser);

                // Create new lockout
                var lockout = new DemoLockout
                {
                    LockedUntil = DateTime.UtcNow.AddHours(DEMO_SESSION_HOURS),
                    CreatedAt = DateTime.UtcNow
                };

                var oldLockout = await _context.DemoLockouts.FirstOrDefaultAsync();
                if (oldLockout != null)
                {
                    _context.DemoLockouts.Remove(oldLockout);
                }

                _context.DemoLockouts.Add(lockout);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TimeSpan> GetTimeUntilUnlockAsync()
        {
            var lockout = await GetCurrentDemoLockoutAsync();
            if (lockout == null || !lockout.IsLocked)
                return TimeSpan.Zero;

            return lockout.LockedUntil - DateTime.UtcNow;
        }

        public async Task<DemoLockout?> GetCurrentDemoLockoutAsync()
        {
            return await _context.DemoLockouts.FirstOrDefaultAsync();
        }
    }
}
