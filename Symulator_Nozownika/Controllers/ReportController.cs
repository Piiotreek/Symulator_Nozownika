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
       

        public ReportController(IReportService reportService, AppDbContext context)
        {
            _reportService = reportService;
            _context = context;
           
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ClubPdf(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var isOwner = await _context.Clubs.AnyAsync(c => c.Id == id && c.OwnerId == userId);

            if (!isAdmin && !isOwner)
                return Forbid();

            var pdf = await _reportService.GenerateClubPdfAsync(id);
            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", $"club-{id}-members.pdf");
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> MyStatisticsPdf()
        {
            var pdf = await _reportService.GenerateUserStatisticsPdfAsync(
                int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"));

            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", $"user-{User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}-statistics.pdf");
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AllStatisticsPdf()
        {
            var pdf = await _reportService.GenerateAllStatisticsPdfAsync();
            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", "all-user-statistics.pdf");
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> HighscoresPdf()
        {
            var pdf = await _reportService.GenerateHighscoresPdfAsync();
            if (pdf.Length == 0) return NotFound();
            return File(pdf, "application/pdf", "highscores.pdf");
        }
    }
}
