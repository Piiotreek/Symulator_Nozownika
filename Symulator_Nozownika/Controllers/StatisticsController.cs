using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using Symulator_Nozownika.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILevelService _levelService;

        public StatisticsController(AppDbContext context, ILevelService levelService)
        {
            _context = context;
            _levelService = levelService;
        }

        // GET: Statistics
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var statistics = await _context.UserStatistics
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (statistics == null)
            {
                // Utwórz nowe statystyki jeśli nie istnieją
                statistics = new UserStatistics
                {
                    UserId = userId,
                    TotalGamesPlayed = 0,
                    TotalScore = 0,
                    HighestScore = 0,
                    TotalPlayTime = TimeSpan.Zero,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastPlayedAt = DateTime.Now
                };

                _context.UserStatistics.Add(statistics);
                await _context.SaveChangesAsync();

                // Reload with User included
                statistics = await _context.UserStatistics
                    .Include(s => s.User)
                    .FirstAsync(s => s.UserId == userId);
            }

            return View(statistics);
        }

        // GET: Statistics/Leaderboard
        public async Task<IActionResult> Leaderboard()
        {
            var topUsers = await _context.UserStatistics
                .Include(s => s.User)
                .OrderByDescending(s => s.HighestScore)
                .ThenByDescending(s => s.TotalScore)
                .Take(50)
                .ToListAsync();

            return View(topUsers);
        }

        // GET: Statistics/SavedScores
        public async Task<IActionResult> SavedScores()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var saved = await _context.SavedScores
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Score)
                .ToListAsync();

            return View(saved);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatistics([FromBody] GameResult? result)
        {
            if (result is null)
            {
                return BadRequest(new { success = false, message = "Brak danych statystyk." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Nieprawidłowe dane." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var statistics = await _context.UserStatistics
                .FirstOrDefaultAsync(s => s.UserId == userId);

            var level = await _context.Levels
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (statistics == null)
            {
                statistics = new UserStatistics
                {
                    UserId = userId,
                    TotalGamesPlayed = 0,
                    TotalScore = 0,
                    HighestScore = 0,
                    TotalPlayTime = TimeSpan.Zero,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastPlayedAt = DateTime.Now
                };

                _context.UserStatistics.Add(statistics);
            }

            if (level == null)
            {
                level = new Level
                {
                    UserId = userId
                };

                _levelService.SyncLevel(level, statistics.TotalScore);
                _context.Levels.Add(level);
            }

            var score = Math.Max(0, result.Score);

            // Diminishing returns (Kenshi-style): XP (tu: TotalScore) maleje wraz z levelem.
            // Aby zachować spójność z zapisem wyniku w GameScoreController, liczymy level z aktualnego TotalScore.
            var currentLevel = _levelService.GetLevelFromTotalScore(statistics.TotalScore);
            var multiplier = _levelService.GetLevelMultiplier(currentLevel);
            var adjustedScore = (int)Math.Round(score * multiplier, MidpointRounding.AwayFromZero);

            statistics.TotalGamesPlayed += 1;
            statistics.TotalScore += adjustedScore;
            statistics.HighestScore = Math.Max(statistics.HighestScore, score);
            statistics.TotalPlayTime += TimeSpan.FromSeconds(Math.Max(0, result.PlayTimeSeconds));
            statistics.LastPlayedAt = DateTime.Now;

            if (result.Won)
            {
                statistics.CurrentStreak++;

                if (statistics.CurrentStreak > statistics.LongestStreak)
                {
                    statistics.LongestStreak = statistics.CurrentStreak;
                }
            }
            else
            {
                statistics.CurrentStreak = 0;
            }

            _levelService.SyncLevel(level, statistics.TotalScore);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                statistics.TotalGamesPlayed,
                statistics.TotalScore,
                statistics.HighestScore
            });
        }

        // Dodaj klasę pomocniczą
        public class GameResult
        {
            [Range(0, int.MaxValue)]
            public int Score { get; set; }

            [Range(0, int.MaxValue)]
            public int PlayTimeSeconds { get; set; }

            public bool Won { get; set; }
        }
    }
}