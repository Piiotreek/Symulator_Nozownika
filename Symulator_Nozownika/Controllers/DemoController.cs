using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using Symulator_Nozownika.Services;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class DemoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDemoService _demoService;

        public DemoController(AppDbContext context, IDemoService demoService)
        {
            _context = context;
            _demoService = demoService;
        }

        [HttpPost]
        public async Task<IActionResult> CheckStatus()
        {
            var (canPlay, message) = await _demoService.CheckDemoStatusAsync();
            return Json(new { canPlay, message });
        }

        [HttpPost]
        public async Task<IActionResult> StartDemo()
        {
            var (canPlay, message) = await _demoService.CheckDemoStatusAsync();
            
            if (!canPlay)
                return Json(new { success = false, error = message });

            var demoUser = await _demoService.GetOrCreateDemoUserAsync();
            if (demoUser == null)
                return Json(new { success = false, error = "Failed to create demo user" });

            // Sign in as demo user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, demoUser.Id.ToString()),
                new Claim(ClaimTypes.Name, demoUser.Email),
                new Claim("Name", demoUser.UserName),
                new Claim("IsDemo", "true"),
                new Claim(ClaimTypes.Role, "DemoUser")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            return Json(new { success = true, userId = demoUser.Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveDemoScore([FromBody] GameScoreRequest request)
        {
            var isDemo = User.FindFirst("IsDemo")?.Value == "true";
            if (!isDemo)
                return BadRequest(new { error = "Not a demo user" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out int parsedUserId))
                return BadRequest(new { error = "Invalid user ID" });

            // Load demo user with statistics
            var demoUser = await _context.UserAccounts
                .Include(u => u.Statistics)
                .FirstOrDefaultAsync(u => u.Id == parsedUserId && u.IsDemo);

            if (demoUser == null)
                return BadRequest(new { error = "Demo user not found" });

            // If already reached limit, reject
            if (demoUser.DemoGamesPlayed >= 3)
                return BadRequest(new { error = "Demo game limit reached" });

            // Update statistics (create if missing)
            if (demoUser.Statistics == null)
            {
                var stats = new UserStatistics
                {
                    UserId = demoUser.Id,
                    TotalGamesPlayed = 0,
                    TotalScore = 0,
                    HighestScore = 0,
                    TotalClicks = 0,
                    TotalPlayTime = TimeSpan.Zero,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastPlayedAt = DateTime.UtcNow
                };
                _context.UserStatistics.Add(stats);
                demoUser.Statistics = stats;
            }



            demoUser.Statistics.TotalGamesPlayed++;
            demoUser.Statistics.TotalScore += request.Score;
            demoUser.Statistics.TotalClicks += request.Clicks;
            demoUser.Statistics.TotalPlayTime += TimeSpan.FromSeconds(request.PlayTimeSeconds);
            demoUser.Statistics.HighestScore = Math.Max(demoUser.Statistics.HighestScore, request.Score);
            demoUser.Statistics.LastPlayedAt = DateTime.UtcNow;

            // Persist statistics update
            await _context.SaveChangesAsync();

            // Increment demo games count and apply potential lock/delete
            await _demoService.IncrementDemoGamesAndCheckLockAsync(demoUser);

            // Clear EF Core change tracker to ensure fresh data read
            _context.ChangeTracker.Clear();

            // Fetch fresh data from database to get the updated DemoGamesPlayed
            var updatedUser = await _context.UserAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == parsedUserId && u.IsDemo);

            if (updatedUser == null)
            {
                // Account was deleted because limit reached -> sign out
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Json(new
                {
                    success = true,
                    isLastGame = true,
                    gamesPlayed = 3,
                    gamesRemaining = 0,
                    message = "Osiągnięto limit 3 gier! Twoje konto demo zostało usunięte.\n\nSpróbuj ponownie za 6 godzin!",
                    shouldRedirect = true
                });
            }

            // Account still exists -> return updated counters
            int gamesPlayed = updatedUser.DemoGamesPlayed;
            int gamesRemaining = Math.Max(0, 3 - gamesPlayed);
            bool isLastGame = gamesPlayed >= 3;

            return Json(new {
                success = true,
                isLastGame = isLastGame,
                gamesPlayed = gamesPlayed,
                gamesRemaining = gamesRemaining,
                message = isLastGame ? "Osiągnięto limit 3 gier! Twoje konto demo zostało usunięte.\n\nSpróbuj ponownie za 6 godzin!" : "Score saved!"
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EndDemoSession()
        {
            var isDemo = User.FindFirst("IsDemo")?.Value == "true";
            if (!isDemo)
                return BadRequest(new { error = "Not a demo user" });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Json(new { success = true });
        }

        /// <summary>
        /// Wylogowuje gracza demo po osiągnięciu limitu 3 gier
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DemoSessionEnded()
        {
            var isDemo = User.FindFirst("IsDemo")?.Value == "true";
            if (!isDemo)
                return BadRequest(new { error = "Not a demo user" });

            // Wyloguj gracza demo
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            // Get the currently authenticated user (if any)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isDemo = User.FindFirst("IsDemo")?.Value == "true";

            // If user is authenticated and is a demo user, get their current game count
            if (isDemo && int.TryParse(userIdClaim, out int userId))
            {
                var demoUser = await _context.UserAccounts
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsDemo);

                if (demoUser != null)
                {
                    return Json(new
                    {
                        gamesPlayed = demoUser.DemoGamesPlayed,
                        canPlayMore = demoUser.DemoGamesPlayed < 3,
                        remainingGames = Math.Max(0, 3 - demoUser.DemoGamesPlayed),
                        isLocked = false
                    });
                }
            }

            // If not authenticated, check lockout status
            var lockout = await _demoService.GetCurrentDemoLockoutAsync();
            if (lockout?.IsLocked == true)
            {
                var timeRemaining = lockout.LockedUntil - DateTime.UtcNow;
                return Json(new
                {
                    isLocked = true,
                    remaining = timeRemaining.TotalSeconds,
                    formattedTime = $"{timeRemaining.Hours}h {timeRemaining.Minutes}m {timeRemaining.Seconds}s",
                    gamesPlayed = 0,
                    canPlayMore = false,
                    remainingGames = 0
                });
            }

            return Json(new
            {
                gamesPlayed = 0,
                canPlayMore = true,
                remainingGames = 3,
                isLocked = false
            });
        }
    }
}
