using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using Symulator_Nozownika.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Symulator_Nozownika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsApiController : ControllerBase
    {
        private readonly IReportManagementService _reportService;
        private readonly AppDbContext _context;
        private readonly ILogger<ReportsApiController> _logger;

        public ReportsApiController(IReportManagementService reportService, AppDbContext context, ILogger<ReportsApiController> logger)
        {
            _reportService = reportService;
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpPost("message")]
        public async Task<IActionResult> ReportMessage([FromBody] ReportMessageRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Brak danych zgłoszenia." });

            if (!ModelState.IsValid)
                return BadRequest(new { error = "Nieprawidłowe dane zgłoszenia.", details = ModelState });

            if (request.MessageId <= 0)
                return BadRequest(new { error = "Nieprawidłowe ID wiadomości." });

            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { error = "Użytkownik niezalogowany." });

            var message = await _context.ClubMessages.FindAsync(request.MessageId);
            if (message == null)
                return NotFound(new { error = $"Wiadomość ID={request.MessageId} nie istnieje." });

            // Prevent reporting your own message
            if (message.UserId == userId)
                return BadRequest(new { error = "Nie możesz zgłosić własnej wiadomości." });

            // Normalize and validate reason
            var normalizedReason = (request.Reason ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedReason))
                return BadRequest(new { error = "Powód zgłoszenia nie może być pusty." });

            // Prevent duplicate pending report from same user
            var existing = await _context.MessageReports
                .FirstOrDefaultAsync(r => r.ReportedMessageId == request.MessageId && r.ReportedByUserId == userId && r.Status == ReportStatus.Pending);
            if (existing != null)
                return BadRequest(new { error = "Już zgłosiłeś tę wiadomość (oczekujące zgłoszenie)." });

            try
            {
                _logger.LogInformation("ReportMessage: attempting to report messageId={MessageId} by userId={UserId} with reason={Reason}", request.MessageId, userId, normalizedReason);
                
                bool success = await _reportService.ReportMessageAsync(request.MessageId, userId, normalizedReason);
                if (success)
                {
                    _logger.LogInformation("ReportMessage: success for messageId={MessageId} by userId={UserId}", request.MessageId, userId);
                    return Ok(new { message = "Wiadomość została zgłoszona." });
                }

                _logger.LogWarning("ReportMessageAsync returned false for messageId={MessageId} userId={UserId}", request.MessageId, userId);
                return StatusCode(500, new { error = "Nie udało się zgłosić wiadomości (service returned false)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reporting message {MessageId} by user {UserId}", request.MessageId, userId);
                return StatusCode(500, new { error = "Nie udało się zgłosić wiadomości.", details = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> ReportUser([FromBody] ReportUserRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Brak danych zgłoszenia." });

            if (!ModelState.IsValid)
                return BadRequest(new { error = "Nieprawidłowe dane zgłoszenia.", details = ModelState });

            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { error = "Użytkownik niezalogowany." });

            try
            {
                bool success = await _reportService.ReportUserAsync(request.ReportedUserId, userId, request.Reason, request.Description);
                if (success)
                    return Ok(new { message = "Użytkownik został zgłoszony." });

                _logger.LogWarning("ReportUserAsync returned false for reportedUserId={ReportedUserId} by user={UserId}", request.ReportedUserId, userId);
                return StatusCode(500, new { error = "Nie udało się zgłosić użytkownika." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reporting user {ReportedUserId} by user {UserId}", request.ReportedUserId, userId);
                return StatusCode(500, new { error = "Wystąpił błąd serwera przy zgłaszaniu użytkownika.", details = ex.Message });
            }
        }

        // Admin endpoints
        [HttpGet("messages/pending")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPendingMessageReports()
        {
            var reports = await _reportService.GetPendingMessageReportsAsync();
            return Ok(reports);
        }

        [HttpGet("messages")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllMessageReports()
        {
            var reports = await _reportService.GetAllMessageReportsAsync();
            return Ok(reports);
        }

        [HttpGet("messages/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetMessageReport(int id)
        {
            var report = await _reportService.GetMessageReportByIdAsync(id);
            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpPost("messages/{id}/approve")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApproveMessageReport(int id, [FromBody] AdminActionRequest request)
        {
            int adminId = GetCurrentUserId();
            bool success = await _reportService.ApproveMessageReportAsync(id, adminId, request.Notes);

            if (success)
                return Ok(new { message = "Zgłoszenie wiadomości zatwierdzono." });

            return BadRequest(new { error = "Nie udało się zatwierdzić zgłoszenia." });
        }

        [HttpPost("messages/{id}/reject")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RejectMessageReport(int id, [FromBody] AdminActionRequest request)
        {
            int adminId = GetCurrentUserId();
            bool success = await _reportService.RejectMessageReportAsync(id, adminId, request.Notes);

            if (success)
                return Ok(new { message = "Zgłoszenie wiadomości odrzucono." });

            return BadRequest(new { error = "Nie udało się odrzucić zgłoszenia." });
        }

        [HttpPost("messages/{id}/penalty")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApplyPenaltyToMessageReport(int id, [FromBody] ApplyPenaltyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int adminId = GetCurrentUserId();

            // Pre-checks to provide meaningful errors
            var report = await _context.MessageReports
                .Include(r => r.ReportedMessage)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return NotFound(new { error = "Zgłoszenie nie istnieje." });

            if (report.ReportedMessage == null)
                return BadRequest(new { error = "Zgłoszona wiadomość nie istnieje." });

            // Validate reason
            var normalizedReason = (request.Reason ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedReason))
                return BadRequest(new { error = "Powód kary nie może być pusty." });

            // Ensure user exists
            var user = await _context.UserAccounts.FindAsync(report.ReportedMessage.UserId);
            if (user == null)
                return BadRequest(new { error = "Autor wiadomości nie istnieje (konto usunięte)." });

            try
            {
                bool success = await _reportService.ApplyPenaltyToMessageReportAsync(id, adminId, request.Type, request.Reason, request.Notes);
                if (success)
                {
                    // Try to fetch the penalty that was just created for visibility
                    var createdPenalty = await _context.UserPenalties
                        .Where(up => up.UserId == report.ReportedMessage.UserId && up.AdminId == adminId)
                        .OrderByDescending(up => up.AppliedAt)
                        .FirstOrDefaultAsync();

                    if (createdPenalty != null)
                    {
                        return Ok(new
                        {
                            message = "Kara została zastosowana, a wiadomość usunięta.",
                            penalty = new
                            {
                                id = createdPenalty.Id,
                                type = createdPenalty.Type,
                                isActive = createdPenalty.IsActive,
                                expiresAt = createdPenalty.ExpiresAt
                            }
                        });
                    }

                    return Ok(new { message = "Kara została zastosowana, a wiadomość usunięta." });
                }

                // If service returned false without exception, return generic but include context
                return BadRequest(new { error = "Nie udało się zastosować kary za wiadomość (serwis nie wykonał operacji)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApplyPenaltyToMessageReport error reportId={ReportId} adminId={AdminId}", id, adminId);
                return StatusCode(500, new { error = "Błąd serwera podczas stosowania kary.", details = ex.Message });
            }
        }

        [HttpGet("users/pending")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPendingUserReports()
        {
            var reports = await _reportService.GetPendingUserReportsAsync();
            return Ok(reports);
        }

        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllUserReports()
        {
            var reports = await _reportService.GetAllUserReportsAsync();
            return Ok(reports);
        }

        [HttpGet("users/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUserReport(int id)
        {
            var report = await _reportService.GetUserReportByIdAsync(id);
            if (report == null)
                return NotFound();

            return Ok(report);
        }

        [HttpPost("users/{id}/approve")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApproveUserReport(int id, [FromBody] AdminActionRequest request)
        {
            int adminId = GetCurrentUserId();
            bool success = await _reportService.ApproveUserReportAsync(id, adminId, request.Notes);

            if (success)
                return Ok(new { message = "Zgłoszenie użytkownika zatwierdzone." });

            return BadRequest(new { error = "Nie udało się zatwierdzić zgłoszenia." });
        }

        [HttpPost("users/{id}/reject")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RejectUserReport(int id, [FromBody] AdminActionRequest request)
        {
            int adminId = GetCurrentUserId();
            bool success = await _reportService.RejectUserReportAsync(id, adminId, request.Notes);

            if (success)
                return Ok(new { message = "Zgłoszenie użytkownika odrzucono." });

            return BadRequest(new { error = "Nie udało się odrzucić zgłoszenia." });
        }

        [HttpPost("penalties")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApplyPenalty([FromBody] ApplyPenaltyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int adminId = GetCurrentUserId();

            // Validate user exists
            var user = await _context.UserAccounts.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new { error = "Użytkownik do ukarania nie istnieje." });

            // Validate reason
            var normalizedReason = (request.Reason ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedReason))
                return BadRequest(new { error = "Powód kary nie może być pusty." });

            // If related report provided, ensure it exists
            if (request.RelatedReportId.HasValue)
            {
                var related = await _context.UserReports.FindAsync(request.RelatedReportId.Value);
                if (related == null)
                    return BadRequest(new { error = "Powiązane zgłoszenie nie istnieje." });
            }

            try
            {
                bool success = await _reportService.ApplyPenaltyAsync(request.UserId, adminId, request.Type, request.Reason, request.RelatedReportId, request.Notes);
                if (success)
                    return Ok(new { message = "Kara została zastosowana." });

                return BadRequest(new { error = "Nie udało się zastosować kary (serwis nie wykonał operacji)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApplyPenalty error userId={UserId} adminId={AdminId}", request.UserId, adminId);
                return StatusCode(500, new { error = "Błąd serwera podczas stosowania kary.", details = ex.Message });
            }
        }

        [HttpGet("penalties")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllPenalties()
        {
            var penalties = await _reportService.GetAllPenaltiesAsync();
            return Ok(penalties);
        }

        [HttpGet("penalties/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetPenalty(int id)
        {
            var penalty = await _reportService.GetPenaltyByIdAsync(id);
            if (penalty == null)
                return NotFound();

            return Ok(penalty);
        }

        [HttpPost("penalties/{id}/revoke")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RevokePenalty(int id)
        {
            bool success = await _reportService.RevokePenaltyAsync(id);

            if (success)
                return Ok(new { message = "Kara została uchylona." });

            return BadRequest(new { error = "Nie udało się uchylić kary." });
        }
    
        // Request DTOs
        public class ReportMessageRequest
        {
            [Required]
            public int MessageId { get; set; }

            [Required]
            public string Reason { get; set; }
        }

        public class ReportUserRequest
        {
            [Required]
            public int ReportedUserId { get; set; }

            [Required]
            public ReportReason Reason { get; set; }

            public string Description { get; set; }
        }

        public class AdminActionRequest
        {
            public string Notes { get; set; }
        }

        public class ApplyPenaltyRequest
        {
            public int UserId { get; set; }
            public PenaltyType Type { get; set; }
            public string Reason { get; set; }
            public int? RelatedReportId { get; set; }
            public string Notes { get; set; } = string.Empty;
        }
    }
}
