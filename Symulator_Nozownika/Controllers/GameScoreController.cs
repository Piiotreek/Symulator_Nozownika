using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class GameScoreRequest
    {
        public int Score { get; set; }
        public string? PlayerName { get; set; }
    }

    public class GameScoreController : Controller
    {
        private readonly AppDbContext _context;

        public GameScoreController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SaveScore([FromBody] GameScoreRequest request)
        {
            var score = request?.Score ?? 0;
            System.Console.WriteLine($"🎮 SaveScore() wywoływana z wynikiem: {score}");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            System.Console.WriteLine($"👤 UserId z Claims: {userId}");

            // Jeśli użytkownik jest zalogowany
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedUserId))
            {
                System.Console.WriteLine($"✅ Użytkownik zalogowany, ID: {parsedUserId}");

                var userAccount = await _context.UserAccounts.FindAsync(parsedUserId);
                if (userAccount != null)
                {
                    System.Console.WriteLine($"👤 Znaleziono użytkownika: {userAccount.FirstName} {userAccount.LastName}");

                    // Sprawdzenie czy nowy wynik jest lepszy niż ostatni
                    var lastHighScore = await _context.HighScores
                        .Where(h => h.UserId == parsedUserId)
                        .OrderByDescending(h => h.Score)
                        .FirstOrDefaultAsync();

                    System.Console.WriteLine($"📊 Ostatni wynik: {lastHighScore?.Score ?? 0}");

                    if (lastHighScore == null || score > lastHighScore.Score)
                    {
                        System.Console.WriteLine($"💾 Zapisuję nowy wynik: {score}");

                        var highScore = new HighScore
                        {
                            UserId = parsedUserId,
                            Score = score,
                            CreatedAt = DateTime.Now,
                            PlayerName = $"{userAccount.FirstName} {userAccount.LastName}"
                        };

                        _context.HighScores.Add(highScore);
                        await _context.SaveChangesAsync();

                        System.Console.WriteLine($"✅ Wynik zapisany pomyślnie!");
                        return Json(new { success = true, message = "Wynik zapisany!", newRecord = lastHighScore == null });
                    }
                    else
                    {
                        System.Console.WriteLine($"⚠️ Wynik {score} nie jest lepszy niż {lastHighScore.Score}");
                        return Json(new { success = false, message = $"Twój najlepszy wynik to {lastHighScore.Score}. Spróbuj jeszcze raz!" });
                    }
                }
                else
                {
                    System.Console.WriteLine($"❌ Użytkownik nie znaleziony w bazie!");
                }
            }
            else
            {
                System.Console.WriteLine($"⚠️ Użytkownik nie zalogowany lub parsowanie ID nie powiodło się");
            }

            // Jeśli nie zalogowany - przechowaj tymczasowo
            System.Console.WriteLine($"↩️ Zwracam needsLogin");
            return Json(new { success = false, needsLogin = true, message = "Zaloguj się aby zapisać swój wynik!" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAnonymousScore([FromBody] GameScoreRequest request)
        {
            var score = request?.Score ?? 0;
            var playerName = request?.PlayerName;

            var highScore = new HighScore
            {
                Score = score,
                CreatedAt = DateTime.Now,
                PlayerName = playerName ?? "Anonimowy gracz",
                UserId = null
            };

            _context.HighScores.Add(highScore);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Wynik zapisany!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTopScores(int limit = 10)
        {
            var topScores = await _context.HighScores
                .OrderByDescending(h => h.Score)
                .Take(limit)
                .Select(h => new
                {
                    h.Id,
                    h.Score,
                    h.CreatedAt,
                    PlayerName = h.PlayerName ?? (h.UserAccount != null ? $"{h.UserAccount.FirstName} {h.UserAccount.LastName}" : "Anonimowy"),
                    h.UserId
                })
                .ToListAsync();

            return Json(topScores);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserHighScore()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
            {
                return Json(new { success = false, highScore = 0 });
            }

            var bestScore = await _context.HighScores
                .Where(h => h.UserId == parsedUserId)
                .MaxAsync(h => (int?)h.Score) ?? 0;

            return Json(new { success = true, highScore = bestScore });
        }

        [HttpGet]
        public async Task<IActionResult> Highscores()
        {
            var topScores = await _context.HighScores
                .OrderByDescending(h => h.Score)
                .Take(50)
                .Select(h => new
                {
                    h.Id,
                    h.Score,
                    h.CreatedAt,
                    PlayerName = h.PlayerName ?? (h.UserAccount != null ? $"{h.UserAccount.FirstName} {h.UserAccount.LastName}" : "Anonimowy"),
                    h.UserId
                })
                .ToListAsync();

            return View(topScores);
        }
    }
}
