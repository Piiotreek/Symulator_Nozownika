using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika.Services
{
    public class CsvExportAuthorizationHandler : AuthorizationHandler<CsvExportRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public CsvExportAuthorizationHandler(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CsvExportRequirement requirement)
        {
            // Demo users cannot export
            if (context.User.FindFirst("IsDemo")?.Value == "true")
            {
                return;
            }

            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return;
            }

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return;
            }

            // Admins can always export
            if (user.Role == UserRole.Admin)
            {
                context.Succeed(requirement);
                return;
            }

            // For club-specific exports, check if user is club owner
            if (requirement.ClubId.HasValue)
            {
                var isClubOwner = await _context.Clubs
                    .AnyAsync(c => c.Id == requirement.ClubId.Value && c.OwnerId == userId);

                if (isClubOwner)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }

    public class CsvExportRequirement : IAuthorizationRequirement
    {
        public int? ClubId { get; set; }

        public CsvExportRequirement(int? clubId = null)
        {
            ClubId = clubId;
        }
    }
}
