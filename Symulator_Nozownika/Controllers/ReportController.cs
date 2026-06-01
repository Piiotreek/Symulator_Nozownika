using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Services;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public ReportController(IReportService reportService, AppDbContext context, IAuthorizationService authorizationService)
        {
            _reportService = reportService;
            _context = context;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = "CanExportClubCsv")]
        public async Task<IActionResult> ClubCsv(int id)
        {
            // Additional check: verify if user is club owner for this specific club
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var requirement = new CsvExportRequirement(id);
            var authResult = await _authorizationService.AuthorizeAsync(User, requirement, "CanExportClubCsv");

            if (!authResult.Succeeded)
                return Forbid();

            var csv = await _reportService.GenerateClubCsvAsync(id);
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", $"club-{id}-members.csv");
        }

        [HttpGet]
        [Authorize(Policy = "CanExportCsv")]
        public async Task<IActionResult> MyStatisticsCsv()
        {
            var csv = await _reportService.GenerateUserStatisticsCsvAsync(
                int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"));

            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", $"user-{User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}-statistics.csv");
        }

        [HttpGet]
        [Authorize(Policy = "CanExportCsv")]
        public async Task<IActionResult> AllStatisticsCsv()
        {
            var csv = await _reportService.GenerateAllStatisticsCsvAsync();
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", "all-user-statistics.csv");
        }

        [HttpGet]
        [Authorize(Policy = "CanExportCsv")]
        public async Task<IActionResult> HighscoresCsv()
        {
            var csv = await _reportService.GenerateHighscoresCsvAsync();
            if (csv.Length == 0) return NotFound();
            return File(csv, "text/csv", "highscores.csv");
        }
    }
}
