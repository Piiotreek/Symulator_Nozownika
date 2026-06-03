using System.Text;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IBrowserService _browserService;

        public ReportService(AppDbContext context, IBrowserService browserService)
        {
            _context = context;
            _browserService = browserService;
        }

        private async Task<byte[]> HtmlToPdfAsync(string htmlContent)
        {
            try
            {
                var browser = await _browserService.GetBrowserAsync();
                using var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20px",
                        Bottom = "20px",
                        Left = "20px",
                        Right = "20px"
                    }
                });
                return pdfBytes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating PDF from HTML", ex);
            }
        }

        private string GenerateClubHtml(Club club)
        {
            var html = new StringBuilder();
            html.Append(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Club Report</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            color: #333;
            background-color: #f5f5f5;
            padding: 20px;
        }
        .container {
            max-width: 900px;
            margin: 0 auto;
            background-color: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .header {
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 3px solid #007bff;
            padding-bottom: 20px;
        }
        h1 {
            color: #007bff;
            font-size: 28px;
            margin-bottom: 10px;
        }
        .info-section {
            margin-bottom: 30px;
            padding: 15px;
            background-color: #f8f9fa;
            border-left: 4px solid #007bff;
        }
        .info-section p {
            margin: 8px 0;
            font-size: 14px;
        }
        .info-label {
            font-weight: bold;
            color: #007bff;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        thead {
            background-color: #343a40;
            color: white;
        }
        th {
            padding: 12px;
            text-align: left;
            font-weight: 600;
            font-size: 13px;
        }
        td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
            font-size: 13px;
        }
        tbody tr:hover {
            background-color: #f8f9fa;
        }
        .footer {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Club Report: ");
            html.Append(club.Name);
            html.Append(@"</h1>
        </div>

        <div class='info-section'>
            <p><span class='info-label'>Owner:</span> ");
            html.Append(club.Owner?.FirstName ?? "N/A");
            html.Append(" ");
            html.Append(club.Owner?.LastName ?? "");
            html.Append(@"</p>
            <p><span class='info-label'>Created:</span> ");
            html.Append(club.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
            html.Append(@"</p>
            <p><span class='info-label'>Total Members:</span> ");
            html.Append(club.Members.Count);
            html.Append(@"</p>
        </div>

        <table>
            <thead>
                <tr>
                    <th>MemberId</th>
                    <th>Username</th>
                    <th>Full Name</th>
                    <th>Country</th>
                    <th>Role</th>
                    <th>Joined</th>
                </tr>
            </thead>
            <tbody>");

            foreach (var member in club.Members.OrderBy(m => m.JoinedAt))
            {
                var user = member.User;
                html.Append(@"<tr>
                    <td>");
                html.Append(member.Id);
                html.Append(@"</td>
                    <td>");
                html.Append(user?.UserName ?? "N/A");
                html.Append(@"</td>
                    <td>");
                html.Append(user?.FirstName ?? "");
                html.Append(" ");
                html.Append(user?.LastName ?? "");
                html.Append(@"</td>
                    <td>");
                html.Append(user?.Country ?? "N/A");
                html.Append(@"</td>
                    <td>");
                html.Append(member.Role);
                html.Append(@"</td>
                    <td>");
                html.Append(member.JoinedAt.ToString("dd.MM.yyyy HH:mm"));
                html.Append(@"</td>
                </tr>");
            }

            html.Append(@"
            </tbody>
        </table>

        <div class='footer'>
            Generated on ");
            html.Append(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"));
            html.Append(@"
        </div>
    </div>
</body>
</html>");

            return html.ToString();
        }

        private string GenerateUserStatisticsHtml(UserStatistics stats)
        {
            var user = stats.User;
            var html = new StringBuilder();
            html.Append(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>User Statistics Report</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; background-color: #f5f5f5; padding: 20px; }
        .container { max-width: 900px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; border-bottom: 3px solid #28a745; padding-bottom: 20px; }
        h1 { color: #28a745; font-size: 28px; margin-bottom: 10px; }
        .info-section { margin-bottom: 30px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #28a745; }
        .info-section p { margin: 8px 0; font-size: 14px; }
        .info-label { font-weight: bold; color: #28a745; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        thead { background-color: #343a40; color: white; }
        th { padding: 12px; text-align: left; font-weight: 600; font-size: 13px; }
        td { padding: 12px; border-bottom: 1px solid #dee2e6; font-size: 13px; }
        .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; text-align: center; color: #6c757d; font-size: 12px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>Raport Statystyk U¿ytkownika</h1></div>
        <div class='info-section'>
            <p><span class='info-label'>Gracz:</span> ");
            html.Append(user?.FirstName ?? "");
            html.Append(" ");
            html.Append(user?.LastName ?? "");
            html.Append(@"</p>
            <p><span class='info-label'>Nazwa U¿ytkownika:</span> ");
            html.Append(user?.UserName ?? "N/A");
            html.Append(@"</p>
            <p><span class='info-label'>Kraj:</span> ");
            html.Append(user?.Country ?? "N/A");
            html.Append(@"</p>
        </div>
        <table>
            <thead><tr><th>Statystyka</th><th>Wartoœæ</th></tr></thead>
            <tbody>
                <tr><td>£¹czna Liczba Gier</td><td>");
            html.Append(stats.TotalGamesPlayed);
            html.Append(@"</td></tr>
                <tr><td>£¹czny Wynik</td><td>");
            html.Append(stats.TotalScore);
            html.Append(@"</td></tr>
                <tr><td>Najwy¿szy Wynik</td><td>");
            html.Append(stats.HighestScore);
            html.Append(@"</td></tr>
                <tr><td>£¹czna Liczba Klikniêæ</td><td>");
            html.Append(stats.TotalClicks);
            html.Append(@"</td></tr>
                <tr><td>£¹czny Czas Gry</td><td>");
            html.Append($"{stats.TotalPlayTime.TotalHours:F2} godz.");
            html.Append(@"</td></tr>
                <tr><td>Aktualna Seria</td><td>");
            html.Append(stats.CurrentStreak);
            html.Append(@"</td></tr>
                <tr><td>Najd³u¿sza Seria</td><td>");
            html.Append(stats.LongestStreak);
            html.Append(@"</td></tr>
                <tr><td>Ostatnia Gra</td><td>");
            html.Append(stats.LastPlayedAt.ToString("dd.MM.yyyy HH:mm"));
            html.Append(@"</td></tr>
            </tbody>
        </table>
        <div class='footer'>Wygenerowano dnia ");
            html.Append(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"));
            html.Append(@"</div>
    </div>
</body>
</html>");

            return html.ToString();
        }

        private string GenerateAllStatisticsHtml(List<UserStatistics> all)
        {
            var html = new StringBuilder();
            html.Append(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>All Users Statistics Report</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; background-color: #f5f5f5; padding: 20px; }
        .container { max-width: 1000px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; border-bottom: 3px solid #ffc107; padding-bottom: 20px; }
        h1 { color: #ffc107; font-size: 28px; margin-bottom: 10px; }
        .info-section { margin-bottom: 20px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #ffc107; }
        .info-label { font-weight: bold; color: #ffc107; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        thead { background-color: #343a40; color: white; }
        th { padding: 12px; text-align: left; font-weight: 600; font-size: 13px; }
        td { padding: 12px; border-bottom: 1px solid #dee2e6; font-size: 13px; }
        .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; text-align: center; color: #6c757d; font-size: 12px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>All Users Statistics Report</h1></div>
        <div class='info-section'>
            <p><span class='info-label'>Total Users:</span> ");
            html.Append(all.Count);
            html.Append(@"</p>
        </div>
        <table>
            <thead>
                <tr>
                    <th>Username</th><th>Full Name</th><th>Total Score</th>
                    <th>Games</th><th>Highest</th><th>Streak</th>
                </tr>
            </thead>
            <tbody>");

            foreach (var stats in all.OrderByDescending(s => s.TotalScore))
            {
                var user = stats.User;
                html.Append(@"<tr><td>");
                html.Append(user?.UserName ?? "N/A");
                html.Append(@"</td><td>");
                html.Append(user?.FirstName ?? "");
                html.Append(" ");
                html.Append(user?.LastName ?? "");
                html.Append(@"</td><td>");
                html.Append(stats.TotalScore);
                html.Append(@"</td><td>");
                html.Append(stats.TotalGamesPlayed);
                html.Append(@"</td><td>");
                html.Append(stats.HighestScore);
                html.Append(@"</td><td>");
                html.Append(stats.LongestStreak);
                html.Append(@"</td></tr>");
            }

            html.Append(@"
            </tbody>
        </table>
        <div class='footer'>Generated on ");
            html.Append(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"));
            html.Append(@"</div>
    </div>
</body>
</html>");

            return html.ToString();
        }

        private string GenerateHighscoresHtml(List<HighScore> highscores)
        {
            var html = new StringBuilder();
            html.Append(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Highscores Report</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; background-color: #f5f5f5; padding: 20px; }
        .container { max-width: 900px; margin: 0 auto; background-color: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; border-bottom: 3px solid #dc3545; padding-bottom: 20px; }
        h1 { color: #dc3545; font-size: 28px; margin-bottom: 10px; }
        .info-section { margin-bottom: 20px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #dc3545; }
        .info-label { font-weight: bold; color: #dc3545; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        thead { background-color: #343a40; color: white; }
        th { padding: 12px; text-align: left; font-weight: 600; font-size: 13px; }
        td { padding: 12px; border-bottom: 1px solid #dee2e6; font-size: 13px; }
        .medal { font-weight: bold; padding: 4px 8px; border-radius: 4px; display: inline-block; min-width: 30px; text-align: center; }
        .gold { background-color: #ffc107; color: #000; }
        .silver { background-color: #c0c0c0; color: #000; }
        .bronze { background-color: #cd7f32; color: #fff; }
        .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; text-align: center; color: #6c757d; font-size: 12px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>Raport Wyników</h1></div>
        <div class='info-section'>
            <p><span class='info-label'>£¹czne Wyniki:</span> ");
            html.Append(highscores.Count);
            html.Append(@"</p>
        </div>
        <table>
            <thead>
                <tr>
                    <th>Ranking</th><th>Gracz</th><th>Kraj</th><th>Wynik</th><th>Data</th>
                </tr>
            </thead>
            <tbody>");

            int pos = 1;
            foreach (var h in highscores)
            {
                var playerName = h.PlayerName ?? (h.UserAccount != null ? $"{h.UserAccount.FirstName} {h.UserAccount.LastName}" : "Anonymous");
                var country = h.Country ?? "N/A";
                var medalClass = pos == 1 ? "gold" : pos == 2 ? "silver" : pos == 3 ? "bronze" : "";

                html.Append(@"<tr>
                    <td><span class='medal ");
                html.Append(medalClass);
                html.Append(@"'>");
                html.Append(pos);
                html.Append(@"</span></td>
                    <td>");
                html.Append(playerName);
                html.Append(@"</td>
                    <td>");
                html.Append(country);
                html.Append(@"</td>
                    <td>");
                html.Append(h.Score);
                html.Append(@"</td>
                    <td>");
                html.Append(h.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                html.Append(@"</td>
                </tr>");
                pos++;
            }

            html.Append(@"
            </tbody>
        </table>
        <div class='footer'>Wygenerowano dnia ");
            html.Append(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"));
            html.Append(@"</div>
    </div>
</body>
</html>");

            return html.ToString();
        }

        public async Task<byte[]> GenerateClubPdfAsync(int clubId)
        {
            var club = await _context.Clubs
                .Include(c => c.Owner)
                .Include(c => c.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null)
                return Array.Empty<byte>();

            string htmlContent = GenerateClubHtml(club);
            return await HtmlToPdfAsync(htmlContent);
        }

        public async Task<byte[]> GenerateUserStatisticsPdfAsync(int userId)
        {
            var stats = await _context.UserStatistics
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (stats == null)
                return Array.Empty<byte>();

            string htmlContent = GenerateUserStatisticsHtml(stats);
            return await HtmlToPdfAsync(htmlContent);
        }

        public async Task<byte[]> GenerateAllStatisticsPdfAsync()
        {
            var all = await _context.UserStatistics
                .Include(s => s.User)
                .ToListAsync();

            string htmlContent = GenerateAllStatisticsHtml(all);
            return await HtmlToPdfAsync(htmlContent);
        }

        public async Task<byte[]> GenerateHighscoresPdfAsync()
        {
            var highscores = await _context.HighScores
                .Include(h => h.UserAccount)
                .OrderByDescending(h => h.Score)
                .ToListAsync();

            string htmlContent = GenerateHighscoresHtml(highscores);
            return await HtmlToPdfAsync(htmlContent);
        }
    }
}
