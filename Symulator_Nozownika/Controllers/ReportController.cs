using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symulator_Nozownika.Services;

namespace Symulator_Nozownika.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> ClubCsv(int id)
        {
            var csv = await _reportService.GenerateClubCsvAsync(id);
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", $"club-{id}-members.csv");
        }

        [HttpGet]
        public async Task<IActionResult> MyStatisticsCsv()
        {
            // Block demo users from exporting statistics
            if (User.FindFirst("IsDemo")?.Value == "true")
                return Unauthorized();

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var csv = await _reportService.GenerateUserStatisticsCsvAsync(userId);
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", $"user-{userId}-statistics.csv");
        }

        [HttpGet]
        public async Task<IActionResult> AllStatisticsCsv()
        {
            // Block demo users from exporting statistics
            if (User.FindFirst("IsDemo")?.Value == "true")
                return Unauthorized();

            var csv = await _reportService.GenerateAllStatisticsCsvAsync();
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", "all-user-statistics.csv");
        }

        [HttpGet]
        public async Task<IActionResult> HighscoresCsv()
        {
            // Block demo users from exporting highscores
            if (User.FindFirst("IsDemo")?.Value == "true")
                return Unauthorized();

            var csv = await _reportService.GenerateHighscoresCsvAsync();
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", "highscores.csv");
        }
    }
}
