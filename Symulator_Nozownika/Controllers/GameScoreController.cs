using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class GameScoreRequest
    {
        public int Score { get; set; }
        public string? PlayerName { get; set; }
        public int PlayTimeSeconds { get; set; }
        public bool Won { get; set; }
    }

    public class GameScoreController : Controller
    {
        private sealed class CountryCatalogItem
        {
            public string? Name { get; set; }
            public string? Flag { get; set; }
            public string? Code { get; set; }
        }

        private const string CountriesCatalogUrl = "https://gist.githubusercontent.com/devhammed/78cfbee0c36dfdaa4fce7e79c0d39208/raw/449258552611926be9ee7a8b4acc2ed9b2243a97/countries.json";
        private static List<CountryCatalogItem>? _countriesCache;
        private static readonly SemaphoreSlim _countriesCacheLock = new(1, 1);

        private readonly AppDbContext _context;

        public GameScoreController(AppDbContext context)
        {
            _context = context;
        }

        private static string NormalizeCountryName(string country)
        {
            return country.Trim();
        }

        private static string ApplyCountryAliases(string country)
        {
            return country switch
            {
                "Czechia" => "Czech Republic",
                "North Macedonia" => "Macedonia",
                "South Korea" => "Korea, Republic of South Korea",
                "North Korea" => "Korea, Democratic People's Republic of Korea",
                "Russia" => "Russia",
                "Ivory Coast" => "Côte d'Ivoire",
                _ => country
            };
        }

        private async Task<List<CountryCatalogItem>> GetCountriesCatalogAsync()
        {
            if (_countriesCache != null)
            {
                return _countriesCache;
            }

            await _countriesCacheLock.WaitAsync();
            try
            {
                if (_countriesCache != null)
                {
                    return _countriesCache;
                }

                using var httpClient = new HttpClient();
                var response = await httpClient.GetFromJsonAsync<List<CountryCatalogItem>>(CountriesCatalogUrl);
                _countriesCache = response ?? new List<CountryCatalogItem>();
                return _countriesCache;
            }
            catch
            {
                _countriesCache = new List<CountryCatalogItem>();
                return _countriesCache;
            }
            finally
            {
                _countriesCacheLock.Release();
            }
        }

        private async Task<string?> GetCountryFlagAsync(string? country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return null;
            }

            var normalized = NormalizeCountryName(country);
            var alias = ApplyCountryAliases(normalized);
            var countries = await GetCountriesCatalogAsync();

            var exactMatch = countries.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.Name) &&
                string.Equals(c.Name, alias, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(exactMatch?.Flag))
            {
                return exactMatch.Flag;
            }

            var containsMatch = countries.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.Name) &&
                (c.Name.Contains(alias, StringComparison.OrdinalIgnoreCase) ||
                 alias.Contains(c.Name, StringComparison.OrdinalIgnoreCase)));

            return containsMatch?.Flag;
        }

        private async Task UpdateUserStatisticsAsync(int userId, GameScoreRequest request)
        {
            var statistics = await _context.UserStatistics
                .FirstOrDefaultAsync(s => s.UserId == userId);

            var today = DateTime.Today;

            if (statistics == null)
            {
                statistics = new UserStatistics
                {
                    UserId = userId,
                    TotalGamesPlayed = 0,
                    TotalScore = 0,
                    HighestScore = 0,
                    TotalPlayTime = TimeSpan.Zero,
                    CurrentStreak = 1,
                    LongestStreak = 1,
                    LastPlayedAt = DateTime.Now
                };

                _context.UserStatistics.Add(statistics);
            }
            else
            {
                var lastPlayed = statistics.LastPlayedAt.Date;

                if (lastPlayed == today)
                {
                    // Już grał dziś – streak bez zmian
                }
                else if (lastPlayed == today.AddDays(-1))
                {
                    // Grał wczoraj – kontynuacja streaku
                    statistics.CurrentStreak++;
                    if (statistics.CurrentStreak > statistics.LongestStreak)
                        statistics.LongestStreak = statistics.CurrentStreak;
                }
                else
                {
                    // Przerwa dłuższa niż 1 dzień – reset
                    statistics.CurrentStreak = 1;
                    if (statistics.LongestStreak < 1)
                        statistics.LongestStreak = 1;
                }
            }

            var score = Math.Max(0, request.Score);
            var playTimeSeconds = Math.Max(0, request.PlayTimeSeconds);

            statistics.TotalGamesPlayed += 1;
            statistics.TotalScore += score;
            statistics.HighestScore = Math.Max(statistics.HighestScore, score);
            statistics.TotalPlayTime += TimeSpan.FromSeconds(playTimeSeconds);
            statistics.LastPlayedAt = DateTime.Now;
        }

        [HttpPost]
        public async Task<IActionResult> SaveScore([FromBody] GameScoreRequest request)
        {
            var score = request?.Score ?? 0;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedUserId))
            {
                var userAccount = await _context.UserAccounts.FindAsync(parsedUserId);
                if (userAccount != null)
                {
                    await UpdateUserStatisticsAsync(parsedUserId, request);

                    var userScores = await _context.HighScores
                        .Where(h => h.UserId == parsedUserId)
                        .OrderByDescending(h => h.Score)
                        .ThenByDescending(h => h.CreatedAt)
                        .ToListAsync();

                    var personalBest = userScores.FirstOrDefault();

                    if (userScores.Count > 1)
                    {
                        _context.HighScores.RemoveRange(userScores.Skip(1));
                    }

                    var country = userAccount.Country;
                    var countryFlag = await GetCountryFlagAsync(country);

                    // Jeśli nie było wcześniejszych wyników
                    if (personalBest == null)
                    {
                        var highScore = new HighScore
                        {
                            UserId = parsedUserId,
                            Score = score,
                            CreatedAt = DateTime.Now,
                            PlayerName = $"{userAccount.FirstName} {userAccount.LastName}",
                            Country = country,
                            CountryFlag = countryFlag
                        };

                        _context.HighScores.Add(highScore);
                        await _context.SaveChangesAsync();
                        System.Console.WriteLine($"✅ Wynik zapisany pomyślnie!");
                        return Json(new { success = true, message = "Wynik zapisany!", newRecord = true });
                    }

                    // Jeśli nowy wynik jest lepszy od najlepszego
                    if (score > personalBest.Score)
                    {
                        personalBest.Score = score;
                        personalBest.CreatedAt = DateTime.Now;
                        personalBest.PlayerName = $"{userAccount.FirstName} {userAccount.LastName}";
                        personalBest.Country = country;
                        personalBest.CountryFlag = countryFlag;

                        await _context.SaveChangesAsync();
                        System.Console.WriteLine($"✅ Nowy Personal Best zapisany!");
                        return Json(new { success = true, message = "Nowy Personal Best zapisany!", newRecord = false });
                    }

                    // Zapisz statystyki (np. streak) nawet jeśli wynik nie jest nowym rekordem
                    await _context.SaveChangesAsync();

                    System.Console.WriteLine($"⚠️ Wynik {score} nie jest lepszy niż {personalBest.Score}");
                    return Json(new { success = false, message = $"Twój najlepszy wynik to {personalBest.Score}. Spróbuj jeszcze raz!" });
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
                UserId = null,
                Country = null,
                CountryFlag = null
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
                    h.UserId,
                    h.Country,
                    h.CountryFlag
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
                    h.UserId,
                    h.Country,
                    h.CountryFlag
                })
                .ToListAsync();

            return View(topScores);
        }

        [HttpGet]
        [Route("api/highscores")]
        public async Task<IActionResult> GetHighscoresApi(string? sortBy = "score", string? direction = "desc", int limit = 50)
        {
            IQueryable<HighScore> query = _context.HighScores;

            // Sort
            query = sortBy?.ToLower() switch
            {
                "playerName" or "playername" => direction?.ToLower() == "asc"
                    ? query.OrderBy(h => h.PlayerName)
                    : query.OrderByDescending(h => h.PlayerName),
                "country" => direction?.ToLower() == "asc"
                    ? query.OrderBy(h => h.Country)
                    : query.OrderByDescending(h => h.Country),
                "date" or "createdat" => direction?.ToLower() == "asc"
                    ? query.OrderBy(h => h.CreatedAt)
                    : query.OrderByDescending(h => h.CreatedAt),
                _ => direction?.ToLower() == "asc"
                    ? query.OrderBy(h => h.Score)
                    : query.OrderByDescending(h => h.Score)
            };

            var highscoresData = await query
                .Take(Math.Min(limit, 500))
                .ToListAsync();  // Wykonaj zapytanie do DB

            var highscores = highscoresData
                .Select((h, index) => new  // Oblicz indeks na kliencie
                {
                    position = index + 1,
                    h.Id,
                    h.Score,
                    h.CreatedAt,
                    PlayerName = h.PlayerName ?? (h.UserAccount != null ? $"{h.UserAccount.FirstName} {h.UserAccount.LastName}" : "Anonimowy"),
                    h.UserId,
                    h.Country,
                    h.CountryFlag
                })
                .ToList();

            return Json(new
            {
                success = true,
                count = highscores.Count,
                data = highscores
            });
        }

        [HttpPost]
        public async Task<IActionResult> PinScore([FromBody] PinScoreRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                return Json(new { success = false, message = "Musisz być zalogowany." });

            var saved = new SavedScore
            {
                UserId = parsedUserId,
                Score = request.Score,
                AchievedAt = DateTime.Now,
                SavedAt = DateTime.Now,
                Note = request.Note
            };

            _context.SavedScores.Add(saved);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Wynik zapisany do ulubionych!", id = saved.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UnpinScore([FromBody] UnpinScoreRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                return Json(new { success = false, message = "Musisz być zalogowany." });

            var saved = await _context.SavedScores
                .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == parsedUserId);

            if (saved == null)
                return Json(new { success = false, message = "Nie znaleziono wyniku." });

            _context.SavedScores.Remove(saved);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Wynik usunięty z ulubionych." });
        }
    }
}

public class PinScoreRequest
{
    public int Score { get; set; }
    public string? Note { get; set; }
}

public class UnpinScoreRequest
{
    public int Id { get; set; }
}
