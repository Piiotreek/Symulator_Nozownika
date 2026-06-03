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
        public async Task<IActionResult> ClubPdf(int id)
        {
            // Additional check: verify if user is club owner for this specific club
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var requirement = new CsvExportRequirement(id);
            var authResult = await _authorizationService.AuthorizeAsync(User, requirement, "CanExportClubCsv");

            if (!authResult.Succeeded)
                return Forbid();

            var pdf = await _reportService.GenerateClubPdfAsync(id);
            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", $"club-{id}-members.pdf");
        }

        [HttpGet]
        [Authorize(Policy = "CanExportCsv")]
        public async Task<IActionResult> MyStatisticsPdf()
        {
            var pdf = await _reportService.GenerateUserStatisticsPdfAsync(
                int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"));

            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", $"user-{User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}-statistics.pdf");
        }

        [HttpGet]
        [Authorize(Policy = "CanExportCsv")]
        public async Task<IActionResult> AllStatisticsPdf()
        {
            var pdf = await _reportService.GenerateAllStatisticsPdfAsync();
            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", "all-user-statistics.pdf");
        }

        [HttpGet]
        [Authorize(Policy = "CanExportCsv")]
        public async Task<IActionResult> HighscoresPdf()
        {
            var pdf = await _reportService.GenerateHighscoresPdfAsync();
            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", "highscores.pdf");
        }
    }
}
