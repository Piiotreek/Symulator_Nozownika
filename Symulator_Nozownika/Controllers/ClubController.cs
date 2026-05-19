using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class ClubController : Controller
    {
        private readonly AppDbContext _context;

        public ClubController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var clubs = await _context.Clubs
                .Include(c => c.Owner)
                .Include(c => c.Members)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var clubList = clubs.Select(c => new ClubListViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                OwnerName = $"{c.Owner.FirstName} {c.Owner.LastName}",
                MemberCount = c.Members.Count,
                AvailableSpots = c.GetAvailableSpots(),
                MaxMembers = c.MaxMembers,
                CreatedAt = c.CreatedAt,
                IsFull = c.IsFull(),
                IsMember = userId.HasValue && c.Members.Any(m => m.UserId == userId.Value),
                IsOwner = userId.HasValue && c.IsOwner(userId.Value)
            }).ToList();

            return View(clubList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            var club = await _context.Clubs
                .Include(c => c.Owner)
                .Include(c => c.Members)
                    .ThenInclude(cm => cm.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
            {
                return NotFound();
            }

            var clubDetails = new ClubDetailsViewModel
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                OwnerId = club.OwnerId,
                OwnerName = $"{club.Owner.FirstName} {club.Owner.LastName}",
                CreatedAt = club.CreatedAt,
                MemberCount = club.Members.Count,
                AvailableSpots = club.GetAvailableSpots(),
                MaxMembers = club.MaxMembers,
                IsFull = club.IsFull(),
                IsOwner = userId.HasValue && club.IsOwner(userId.Value),
                IsMember = userId.HasValue && club.Members.Any(m => m.UserId == userId.Value),
                Members = club.Members.Select(m => new ClubMemberViewModel
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = $"{m.User.FirstName} {m.User.LastName}",
                    UserName = m.User.UserName,
                    Country = m.User.Country,
                    Role = m.Role
                }).ToList()
            };

            return View(clubDetails);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!GetCurrentUserId().HasValue)
            {
                return Redirect("/Account/Login");
            }

            return View(new CreateClubViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateClubViewModel model)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.UserAccounts
                .Include(u => u.Level)
                .Include(u => u.CoinWallet)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            // Sprawdzenie czy użytkownik już ma klub
            if (user.ClubId.HasValue)
            {
                ModelState.AddModelError("", "You are already a member of a club. Leave your current club first.");
                return View(model);
            }

            // Sprawdzenie wymagań do utworzenia klubu (level 7 lub 500 gold)
            var userLevel = user.Level?.CurrentLevel ?? 1;
            var userGold = user.CoinWallet?.Balance ?? 0;
            if (userLevel < 7 && userGold < 500)
            {
                ModelState.AddModelError("", "You need at least level 7 or 500 coins to create a club.");
                return View(model);
            }

            // Sprawdzenie czy klub o tej nazwie już istnieje
            var existingClub = await _context.Clubs
                .FirstOrDefaultAsync(c => c.Name == model.Name);
            if (existingClub != null)
            {
                ModelState.AddModelError("", "A club with this name already exists.");
                return View(model);
            }

            var club = new Club
            {
                Name = model.Name,
                Description = model.Description,
                OwnerId = userId.Value,
                OwnerName = $"{user.FirstName} {user.LastName}",
                CreatedAt = DateTime.Now,
                MaxMembers = 20
            };

            var clubMember = new ClubMember
            {
                UserId = userId.Value,
                User = user,
                Club = club,
                Role = ClubRole.Owner,
                JoinedAt = DateTime.Now
            };

            club.Members.Add(clubMember);
            user.ClubId = null;

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = club.Id });
        }

        [HttpPost]
        public async Task<IActionResult> JoinClub(int clubId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/Account/Login");
            }

            var club = await _context.Clubs
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null)
            {
                return NotFound();
            }

            var user = await _context.UserAccounts.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            // Sprawdzenie czy już jest członkiem
            if (user.ClubId.HasValue)
            {
                TempData["Error"] = "You are already a member of a club.";
                return RedirectToAction("Details", new { id = clubId });
            }

            // Sprawdzenie czy klub jest pełny
            if (club.IsFull())
            {
                TempData["Error"] = "This club is full.";
                return RedirectToAction("Details", new { id = clubId });
            }

            user.ClubId = clubId;
            var clubMember = new ClubMember
            {
                UserId = userId.Value,
                User = user,
                Club = club,
                Role = ClubRole.Member,
                JoinedAt = DateTime.Now
            };
            club.Members.Add(clubMember);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"You have successfully joined {club.Name}!";
            return RedirectToAction("Details", new { id = clubId });
        }

        [HttpPost]
        public async Task<IActionResult> LeaveClub(int clubId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/Account/Login");
            }

            var club = await _context.Clubs
                .Include(c => c.Members)
                    .ThenInclude(cm => cm.User)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null)
            {
                return NotFound();
            }

            var user = await _context.UserAccounts.FindAsync(userId.Value);
            if (user == null || user.ClubId != clubId)
            {
                return NotFound();
            }

            // Sprawdzenie czy użytkownik jest właścicielem
            if (club.IsOwner(userId.Value))
            {
                // Jeśli owner odchodzi - przesuń uprawnienia na najstarszego członka (po ownerze)
                var oldestMember = club.Members
                    .Where(m => m.UserId != userId.Value)
                    .OrderBy(m => m.JoinedAt)
                    .FirstOrDefault();

                if (oldestMember != null)
                {
                    oldestMember.Role = ClubRole.Owner;
                    club.OwnerId = oldestMember.UserId;
                    club.OwnerName = $"{oldestMember.User.FirstName} {oldestMember.User.LastName}";
                    TempData["Info"] = $"Ownership transferred to {oldestMember.User.UserName}.";
                }
                else
                {
                    // Jeśli owner to jedyny członek - usuń klub
                    _context.Clubs.Remove(club);
                    user.ClubId = null;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Club has been deleted because you were the only member.";
                    return RedirectToAction("Index");
                }
            }

            user.ClubId = null;
            var memberToRemove = club.Members.FirstOrDefault(m => m.UserId == userId.Value);
            if (memberToRemove != null)
            {
                club.Members.Remove(memberToRemove);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "You have left the club.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClub(int clubId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/Account/Login");
            }

            var club = await _context.Clubs
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null)
            {
                return NotFound();
            }

            if (!club.IsOwner(userId.Value))
            {
                TempData["Error"] = "Only the club owner can delete the club.";
                return RedirectToAction("Details", new { id = clubId });
            }

            // Usunięcie powiązań członków
            foreach (var member in club.Members)
            {
                if (member.User != null)
                {
                    member.User.ClubId = null;
                }
            }

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Club has been deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int clubId, int memberId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/Account/Login");
            }

            var club = await _context.Clubs
                .Include(c => c.Members)
                    .ThenInclude(cm => cm.User)
                .FirstOrDefaultAsync(c => c.Id == clubId);

            if (club == null)
            {
                return NotFound();
            }

            // Sprawdzenie czy użytkownik jest ownerem lub moderatorem
            var currentUserMember = club.Members.FirstOrDefault(m => m.UserId == userId.Value);
            if (currentUserMember?.Role != ClubRole.Owner && currentUserMember?.Role != ClubRole.Moderator)
            {
                TempData["Error"] = "Only the club owner or moderator can remove members.";
                return RedirectToAction("Details", new { id = clubId });
            }

            // Nie można usunąć ownera (chyba że sam się usuwa)
            if (memberId == club.OwnerId && userId.Value != club.OwnerId)
            {
                TempData["Error"] = "You cannot remove the club owner.";
                return RedirectToAction("Details", new { id = clubId });
            }

            var member = await _context.UserAccounts.FindAsync(memberId);
            if (member == null || member.ClubId != clubId)
            {
                return NotFound();
            }

            member.ClubId = null;
            var clubMemberToRemove = club.Members.FirstOrDefault(m => m.UserId == memberId);
            if (clubMemberToRemove != null)
            {
                club.Members.Remove(clubMemberToRemove);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{member.FirstName} {member.LastName} has been removed from the club.";
            return RedirectToAction("Details", new { id = clubId });
        }

        [HttpGet]
        public async Task<IActionResult> Chat(int clubId)
        {
            var club = await _context.Clubs.FindAsync(clubId);
            if (club == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Redirect("/Account/Login");

            var isMember = await _context.ClubMembers
                .Where(cm => cm.UserId == userId && cm.ClubId == clubId)
                .AnyAsync();

            if (!isMember)
                return Forbid();

            var messages = await _context.ClubMessages
                .Where(m => m.ClubId == clubId)
                .Include(m => m.User)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.ClubId = clubId;
            ViewBag.ClubName = club.Name;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int clubId, string content)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Redirect("/Account/Login");

            var club = await _context.Clubs.FindAsync(clubId);
            if (club == null)
                return NotFound();

            var isMember = await _context.ClubMembers
                .Where(cm => cm.UserId == userId && cm.ClubId == clubId)
                .AnyAsync();

            if (!isMember)
                return Forbid();

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Wiadomość nie może być pusta.";
                return RedirectToAction("Chat", new { clubId });
            }

            var message = new ClubMessage
            {
                ClubId = clubId,
                UserId = userId.Value,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ClubMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { clubId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId, int clubId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var message = await _context.ClubMessages.FindAsync(messageId);
            if (message == null)
                return NotFound();

            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
                return NotFound();

            bool isAuthor = message.UserId == userId;
            bool isOwner = club.IsOwner(userId.Value);

            if (!isAuthor && !isOwner)
                return Forbid();

            _context.ClubMessages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wiadomość usunięta.";
            return RedirectToAction("Chat", new { clubId });
        }
    }
}
