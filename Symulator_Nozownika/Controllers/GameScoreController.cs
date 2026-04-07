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

                    var userScores = await _context.HighScores
                        .Where(h => h.UserId == parsedUserId)
                        .OrderByDescending(h => h.Score)
                        .ThenByDescending(h => h.CreatedAt)
                        .ToListAsync();

                    var personalBest = userScores.FirstOrDefault();

                    // Zachowaj tylko jeden najwyższy wynik
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

                    // Jeśli nie ma potrzeby aktualizacji rekordu
                    if (userScores.Count > 1)
                    {
                        await _context.SaveChangesAsync();
                    }

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
    }
}
