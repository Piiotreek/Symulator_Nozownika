using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
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

            var score = Math.Max(0, result.Score);

            statistics.TotalGamesPlayed += 1;
            statistics.TotalScore += score;
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