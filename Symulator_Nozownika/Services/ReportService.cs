using System.Text;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateClubCsvAsync(int clubId)
        {
            var club = await _context.Clubs
                .Include(c => c.Owner)
                .Include(c => c.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null)
                return Array.Empty<byte>();

            var sb = new StringBuilder();
            // Header
            sb.AppendLine("MemberId,UserId,UserName,FullName,Country,Role,JoinedAt");

            foreach (var member in club.Members.OrderBy(m => m.JoinedAt))
            {
                var user = member.User;
                var fullName = user != null ? $"{EscapeCsv(user.FirstName)} {EscapeCsv(user.LastName)}" : "";
                var userName = user != null ? EscapeCsv(user.UserName) : "";
                var country = user != null ? EscapeCsv(user.Country) : "";
                sb.AppendLine($"{member.Id},{member.UserId},\"{userName}\",\"{fullName}\",\"{country}\",{member.Role},{member.JoinedAt:O}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateUserStatisticsCsvAsync(int userId)
        {
            var stats = await _context.UserStatistics
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (stats == null)
                return Array.Empty<byte>();

            var sb = new StringBuilder();
            sb.AppendLine("UserId,UserName,FullName,TotalGamesPlayed,TotalScore,HighestScore,TotalClicks,TotalPlayTime,CurrentStreak,LongestStreak,LastPlayedAt");

            var user = stats.User;
            var fullName = user != null ? $"{EscapeCsv(user.FirstName)} {EscapeCsv(user.LastName)}" : "";
            var userName = user != null ? EscapeCsv(user.UserName) : "";

            sb.AppendLine($"{stats.UserId},\"{userName}\",\"{fullName}\",{stats.TotalGamesPlayed},{stats.TotalScore},{stats.HighestScore},{stats.TotalClicks},{stats.TotalPlayTime.TotalSeconds},{stats.CurrentStreak},{stats.LongestStreak},{stats.LastPlayedAt:O}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateAllStatisticsCsvAsync()
        {
            var all = await _context.UserStatistics
                .Include(s => s.User)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("UserId,UserName,FullName,TotalGamesPlayed,TotalScore,HighestScore,TotalClicks,TotalPlayTime,CurrentStreak,LongestStreak,LastPlayedAt");

            foreach (var stats in all)
            {
                var user = stats.User;
                var fullName = user != null ? $"{EscapeCsv(user.FirstName)} {EscapeCsv(user.LastName)}" : "";
                var userName = user != null ? EscapeCsv(user.UserName) : "";
                sb.AppendLine($"{stats.UserId},\"{userName}\",\"{fullName}\",{stats.TotalGamesPlayed},{stats.TotalScore},{stats.HighestScore},{stats.TotalClicks},{stats.TotalPlayTime.TotalSeconds},{stats.CurrentStreak},{stats.LongestStreak},{stats.LastPlayedAt:O}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateHighscoresCsvAsync()
        {
            var highscores = await _context.HighScores
                .Include(h => h.UserAccount)
                .OrderByDescending(h => h.Score)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Position,HighScoreId,UserId,PlayerName,Country,Score,CreatedAt");

            int pos = 1;
            foreach (var h in highscores)
            {
                var playerName = h.PlayerName ?? (h.UserAccount != null ? $"{EscapeCsv(h.UserAccount.FirstName)} {EscapeCsv(h.UserAccount.LastName)}" : "Anonimowy");
                var country = EscapeCsv(h.Country);
                sb.AppendLine($"{pos},{h.Id},{h.UserId},\"{playerName}\",\"{country}\",{h.Score},{h.CreatedAt:O}");
                pos++;
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\"", "\"\"");
        }
    }
}
