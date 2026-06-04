using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using Microsoft.Extensions.Logging;

namespace Symulator_Nozownika.Services
{
    public interface IReportManagementService
    {
        // Message Reports
        Task<bool> ReportMessageAsync(int messageId, int reportedByUserId, string reason);
        Task<List<MessageReport>> GetPendingMessageReportsAsync();
        Task<List<MessageReport>> GetAllMessageReportsAsync();
        Task<MessageReport> GetMessageReportByIdAsync(int reportId);
        Task<bool> ApproveMessageReportAsync(int reportId, int adminId, string notes);
        Task<bool> RejectMessageReportAsync(int reportId, int adminId, string notes);

        // User Reports
        Task<bool> ReportUserAsync(int reportedUserId, int reportedByUserId, ReportReason reason, string description);
        Task<List<UserReport>> GetPendingUserReportsAsync();
        Task<List<UserReport>> GetAllUserReportsAsync();
        Task<UserReport> GetUserReportByIdAsync(int reportId);
        Task<bool> ApproveUserReportAsync(int reportId, int adminId, string notes);
        Task<bool> RejectUserReportAsync(int reportId, int adminId, string notes);

        // Penalties
        Task<bool> ApplyPenaltyAsync(int userId, int adminId, PenaltyType penaltyType, string reason, int? relatedReportId = null);
        Task<List<UserPenalty>> GetActivePenaltiesForUserAsync(int userId);
        Task<List<UserPenalty>> GetAllPenaltiesAsync();
        Task<UserPenalty> GetPenaltyByIdAsync(int penaltyId);
        Task<bool> RevokePenaltyAsync(int penaltyId);
        Task CheckAndExpirePenaltiesAsync();
        Task<bool> HasActivePenaltyAsync(int userId);
    }

    public class ReportManagementService : IReportManagementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportManagementService> _logger;

        public ReportManagementService(AppDbContext context, ILogger<ReportManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Message Reports
        public async Task<bool> ReportMessageAsync(int messageId, int reportedByUserId, string reason)
        {
            try
            {
                var message = await _context.ClubMessages.FindAsync(messageId);
                if (message == null)
                {
                    _logger.LogWarning("ReportMessageAsync: message not found messageId={MessageId}", messageId);
                    return false;
                }

                // Normalize reason
                var normalizedReason = (reason ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(normalizedReason))
                {
                    _logger.LogWarning("ReportMessageAsync: empty reason for messageId={MessageId} by user={UserId}", messageId, reportedByUserId);
                    return false;
                }

                // Enforce max length defined in model
                if (normalizedReason.Length > 500)
                    normalizedReason = normalizedReason.Substring(0, 500);

                // Prevent duplicate pending report from same user for same message
                var existing = await _context.MessageReports
                    .FirstOrDefaultAsync(r => r.ReportedMessageId == messageId && r.ReportedByUserId == reportedByUserId && r.Status == ReportStatus.Pending);

                if (existing != null)
                {
                    _logger.LogInformation("ReportMessageAsync: duplicate pending report exists for messageId={MessageId} userId={UserId}", messageId, reportedByUserId);
                    return false; // already reported
                }

                var report = new MessageReport
                {
                    ReportedMessageId = messageId,
                    ReportedByUserId = reportedByUserId,
                    Reason = normalizedReason,
                    CreatedAt = DateTime.UtcNow,
                    Status = ReportStatus.Pending
                };

                _context.MessageReports.Add(report);
                await _context.SaveChangesAsync();
                _logger.LogInformation("ReportMessageAsync: report created id={ReportId} messageId={MessageId} by user={UserId}", report.Id, messageId, reportedByUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportMessageAsync exception for messageId={MessageId} by user={UserId}", messageId, reportedByUserId);
                return false;
            }
        }

        public async Task<List<MessageReport>> GetPendingMessageReportsAsync()
        {
            return await _context.MessageReports
                .Where(mr => mr.Status == ReportStatus.Pending)
                .Include(mr => mr.ReportedMessage)
                .ThenInclude(m => m.User)
                .Include(mr => mr.ReportedByUser)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MessageReport>> GetAllMessageReportsAsync()
        {
            return await _context.MessageReports
                .Include(mr => mr.ReportedMessage)
                .ThenInclude(m => m.User)
                .Include(mr => mr.ReportedByUser)
                .Include(mr => mr.ReviewedByAdmin)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();
        }

        public async Task<MessageReport> GetMessageReportByIdAsync(int reportId)
        {
            return await _context.MessageReports
                .Include(mr => mr.ReportedMessage)
                .ThenInclude(m => m.User)
                .Include(mr => mr.ReportedByUser)
                .Include(mr => mr.ReviewedByAdmin)
                .FirstOrDefaultAsync(mr => mr.Id == reportId);
        }

        public async Task<bool> ApproveMessageReportAsync(int reportId, int adminId, string notes)
        {
            try
            {
                var report = await _context.MessageReports.FindAsync(reportId);
                if (report == null)
                {
                    _logger.LogWarning("ApproveMessageReportAsync: report not found id={ReportId}", reportId);
                    return false;
                }

                report.Status = ReportStatus.Approved;
                report.ReviewedByAdminId = adminId;
                report.ReviewedAt = DateTime.UtcNow;
                report.AdminNotes = notes;

                _context.MessageReports.Update(report);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveMessageReportAsync exception id={ReportId}", reportId);
                return false;
            }
        }

        public async Task<bool> RejectMessageReportAsync(int reportId, int adminId, string notes)
        {
            try
            {
                var report = await _context.MessageReports.FindAsync(reportId);
                if (report == null)
                {
                    _logger.LogWarning("RejectMessageReportAsync: report not found id={ReportId}", reportId);
                    return false;
                }

                report.Status = ReportStatus.Rejected;
                report.ReviewedByAdminId = adminId;
                report.ReviewedAt = DateTime.UtcNow;
                report.AdminNotes = notes;

                _context.MessageReports.Update(report);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RejectMessageReportAsync exception id={ReportId}", reportId);
                return false;
            }
        }

        // User Reports
        public async Task<bool> ReportUserAsync(int reportedUserId, int reportedByUserId, ReportReason reason, string description)
        {
            try
            {
                var user = await _context.UserAccounts.FindAsync(reportedUserId);
                if (user == null)
                {
                    _logger.LogWarning("ReportUserAsync: reported user not found id={UserId}", reportedUserId);
                    return false;
                }

                var normalizedDescription = (description ?? string.Empty).Trim();
                if (normalizedDescription.Length > 1000)
                    normalizedDescription = normalizedDescription.Substring(0, 1000);

                var report = new UserReport
                {
                    ReportedUserId = reportedUserId,
                    ReportedByUserId = reportedByUserId,
                    Reason = reason,
                    Description = normalizedDescription,
                    CreatedAt = DateTime.UtcNow,
                    Status = ReportStatus.Pending
                };

                _context.UserReports.Add(report);
                await _context.SaveChangesAsync();
                _logger.LogInformation("ReportUserAsync: report created id={ReportId} reportedUser={ReportedUserId} by user={UserId}", report.Id, reportedUserId, reportedByUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportUserAsync exception for reportedUserId={ReportedUserId} by user={UserId}", reportedUserId, reportedByUserId);
                return false;
            }
        }

        public async Task<List<UserReport>> GetPendingUserReportsAsync()
        {
            return await _context.UserReports
                .Where(ur => ur.Status == ReportStatus.Pending)
                .Include(ur => ur.ReportedUser)
                .Include(ur => ur.ReportedByUser)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<UserReport>> GetAllUserReportsAsync()
        {
            return await _context.UserReports
                .Include(ur => ur.ReportedUser)
                .Include(ur => ur.ReportedByUser)
                .Include(ur => ur.ReviewedByAdmin)
                .OrderByDescending(ur => ur.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserReport> GetUserReportByIdAsync(int reportId)
        {
            return await _context.UserReports
                .Include(ur => ur.ReportedUser)
                .Include(ur => ur.ReportedByUser)
                .Include(ur => ur.ReviewedByAdmin)
                .FirstOrDefaultAsync(ur => ur.Id == reportId);
        }

        public async Task<bool> ApproveUserReportAsync(int reportId, int adminId, string notes)
        {
            try
            {
                var report = await _context.UserReports.FindAsync(reportId);
                if (report == null)
                {
                    _logger.LogWarning("ApproveUserReportAsync: report not found id={ReportId}", reportId);
                    return false;
                }

                report.Status = ReportStatus.Approved;
                report.ReviewedByAdminId = adminId;
                report.ReviewedAt = DateTime.UtcNow;
                report.AdminNotes = notes;

                _context.UserReports.Update(report);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApproveUserReportAsync exception id={ReportId}", reportId);
                return false;
            }
        }

        public async Task<bool> RejectUserReportAsync(int reportId, int adminId, string notes)
        {
            try
            {
                var report = await _context.UserReports.FindAsync(reportId);
                if (report == null)
                {
                    _logger.LogWarning("RejectUserReportAsync: report not found id={ReportId}", reportId);
                    return false;
                }

                report.Status = ReportStatus.Rejected;
                report.ReviewedByAdminId = adminId;
                report.ReviewedAt = DateTime.UtcNow;
                report.AdminNotes = notes;

                _context.UserReports.Update(report);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RejectUserReportAsync exception id={ReportId}", reportId);
                return false;
            }
        }

        // Penalties
        public async Task<bool> ApplyPenaltyAsync(int userId, int adminId, PenaltyType penaltyType, string reason, int? relatedReportId = null)
        {
            try
            {
                var user = await _context.UserAccounts.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("ApplyPenaltyAsync: user not found id={UserId}", userId);
                    return false;
                }

                var penalty = new UserPenalty
                {
                    UserId = userId,
                    AdminId = adminId,
                    Type = penaltyType,
                    Reason = reason,
                    AppliedAt = DateTime.UtcNow,
                    RelatedReportId = relatedReportId,
                    IsActive = true
                };

                // Calculate expiration time based on penalty type
                if (penaltyType == PenaltyType.Suspension1Day)
                    penalty.ExpiresAt = DateTime.UtcNow.AddDays(1);
                else if (penaltyType == PenaltyType.Suspension7Days)
                    penalty.ExpiresAt = DateTime.UtcNow.AddDays(7);
                // Ban and BanWithDeletion never expire (ExpiresAt stays null)

                _context.UserPenalties.Add(penalty);
                await _context.SaveChangesAsync();
                _logger.LogInformation("ApplyPenaltyAsync: penalty id={PenaltyId} user={UserId} admin={AdminId}", penalty.Id, userId, adminId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApplyPenaltyAsync exception for user={UserId}", userId);
                return false;
            }
        }

        public async Task<List<UserPenalty>> GetActivePenaltiesForUserAsync(int userId)
        {
            return await _context.UserPenalties
                .Where(up => up.UserId == userId && up.IsActive)
                .Include(up => up.Admin)
                .OrderByDescending(up => up.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<UserPenalty>> GetAllPenaltiesAsync()
        {
            return await _context.UserPenalties
                .Include(up => up.User)
                .Include(up => up.Admin)
                .Include(up => up.RelatedReport)
                .OrderByDescending(up => up.AppliedAt)
                .ToListAsync();
        }

        public async Task<UserPenalty> GetPenaltyByIdAsync(int penaltyId)
        {
            return await _context.UserPenalties
                .Include(up => up.User)
                .Include(up => up.Admin)
                .Include(up => up.RelatedReport)
                .FirstOrDefaultAsync(up => up.Id == penaltyId);
        }

        public async Task<bool> RevokePenaltyAsync(int penaltyId)
        {
            try
            {
                var penalty = await _context.UserPenalties.FindAsync(penaltyId);
                if (penalty == null)
                {
                    _logger.LogWarning("RevokePenaltyAsync: penalty not found id={PenaltyId}", penaltyId);
                    return false;
                }

                penalty.IsActive = false;
                _context.UserPenalties.Update(penalty);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RevokePenaltyAsync exception id={PenaltyId}", penaltyId);
                return false;
            }
        }

        public async Task CheckAndExpirePenaltiesAsync()
        {
            var expiredPenalties = await _context.UserPenalties
                .Where(up => up.IsActive && up.ExpiresAt.HasValue && up.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var penalty in expiredPenalties)
            {
                penalty.IsActive = false;
            }

            if (expiredPenalties.Count > 0)
            {
                _context.UserPenalties.UpdateRange(expiredPenalties);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasActivePenaltyAsync(int userId)
        {
            return await _context.UserPenalties
                .AnyAsync(up => up.UserId == userId && up.IsActive);
        }
    }
}
