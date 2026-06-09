# Raporty CSV — dokumentacja

Ten dokument opisuje jak korzystać z nowo dodanych funkcji zapisywania raportów do plików CSV oraz jak rozszerzać je w przyszłości.

Dostępne raporty (endpoints):

- /Report/ClubCsv?id={clubId}
  - Zwraca CSV z listą członków klubu (MemberId, UserId, UserName, FullName, Country, Role, JoinedAt).
  - Wymaga by użytkownik był zalogowany. Jeśli klub nie istnieje zwraca 404.

- /Report/MyStatisticsCsv
  - Zwraca CSV z pojedynczym wierszem statystyk aktualnie zalogowanego użytkownika.
  - Pola: UserId, UserName, FullName, TotalGamesPlayed, TotalScore, HighestScore, TotalClicks, TotalPlayTime (s), CurrentStreak, LongestStreak, LastPlayedAt.

- /Report/AllStatisticsCsv
  - Zwraca CSV z wszystkimi użytkownikami i ich statystykami.

- /Report/HighscoresCsv
  - Zwraca CSV z rankingiem wyników (Position,HighScoreId,UserId,PlayerName,Country,Score,CreatedAt).

Jak używać w kontrolerze lub innym miejscu (przykład):

- Wstrzyknięcie serwisu:
  - Dodaj IReportService do konstruktora:
    private readonly IReportService _reportService;

    public SomeController(IReportService reportService)
    {
        _reportService = reportService;
    }

- Generowanie CSV w pamięci (byte[]):
  var csvBytes = await _reportService.GenerateClubCsvAsync(clubId);
  if (csvBytes.Length > 0)
  {
      return File(csvBytes, "text/csv", $"club-{clubId}-members.csv");
  }

Jak dodać raport dla nowej encji:

1. Zaktualizuj IReportService (Symulator_Nozownika/Services/IReportService.cs) dodając nową metodę, np. Task<byte[]> GenerateQuestsCsvAsync();
2. Implementuj metodę w ReportService (Symulator_Nozownika/Services/ReportService.cs). Pobierz dane z DbContext, zbuduj StringBuilder z nagłówkiem i wierszami, użyj EscapeCsv dla pól tekstowych.
3. Dodaj endpoint w ReportController lub nowy kontroler, który wywoła metodę i zwróci File(..., "text/csv", "nazwa.csv").
4. (Opcjonalnie) Dodaj linki w UI, np. w admin panelu lub na stronach klubów/highscore.

Uwagi implementacyjne:
- Wszystkie generowane pliki używają UTF-8.
- Pola tekstowe są zabezpieczone przez EscapeCsv — podwajamy cudzysłowy.
- Daty są zapisywane w formacie ISO 8601 (round-trip "O") aby ułatwić import do arkuszy kalkulacyjnych i dalszą obróbkę.
- Jeśli chcesz dodać filtrowanie lub paginację, rozważ przyjmowanie parametrów w endpointach i aplikowanie ich w zapytaniach EF.

