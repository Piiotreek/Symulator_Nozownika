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

            // ✅ POBIERZ LUB UTWÓRZ STATYSTYKI
            var statistics = await _context.UserStatistics
                .FirstOrDefaultAsync(s => s.UserId == parsedUserId);

            if (statistics == null)
            {
                statistics = new UserStatistics
                {
                    UserId = parsedUserId,
                    TotalKills = 0,
                    TotalDeaths = 0,
                    TotalGamesPlayed = 0,
                    TotalScore = 0,
                    TotalPlayTime = TimeSpan.Zero,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    Points = 0,
                    LastPlayedAt = DateTime.Now
                };
                _context.UserStatistics.Add(statistics);
            }

            // ✅ AKTUALIZUJ STATYSTYKI
            statistics.TotalGamesPlayed++;
            statistics.TotalScore += score;
            statistics.Points += (score / 10); // Przykład: 10 punktów za każde 100 punktów w grze
            statistics.LastPlayedAt = DateTime.Now;

            // Jeśli masz dane o kills/deaths z request, dodaj je tutaj:
            // statistics.TotalKills += request.Kills;
            // statistics.TotalDeaths += request.Deaths;

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

            // Jeśli nie ma potrzeby aktualizacji rekordu - zapisz statystyki
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