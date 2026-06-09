using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using Symulator_Nozownika.Services;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILevelService _levelService;

        private sealed class CountryApiResponse
        {
            public CountryName Name { get; set; } = default!;
        }

        private sealed class CountryName
        {
            public string Common { get; set; } = string.Empty;
        }

        public AccountController(AppDbContext appDbContext, ILevelService levelService)
        {
            _context = appDbContext;
            _levelService = levelService;
        }

        private bool IsStarterWeapon(int weaponId)
        {
            return weaponId == LevelService.StarterWeaponId;
        }

        private async Task<bool> HasPurchasedWeaponAsync(int userId, int weaponId)
        {
            if (IsStarterWeapon(weaponId))
            {
                return true;
            }

            return await _context.PurchasedWeapons.AnyAsync(pw => pw.UserId == userId && pw.WeaponId == weaponId);
        }

        private async Task<int> GetUserLevelAsync(UserAccount user)
        {
            if (user.Level != null)
            {
                return user.Level.CurrentLevel;
            }

            var totalScore = user.Statistics?.TotalScore ?? 0;
            return _levelService.GetLevelFromTotalScore(totalScore);
        }

        private async Task<UserAccount?> GetCurrentUserForShopAsync(string userName)
        {
            return await _context.UserAccounts
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .Include(u => u.CoinWallet)
                .Include(u => u.PurchasedWeapons)
                .Include(u => u.PurchasedPotions)
                .Include(u => u.PurchasedWeaponUpgrades)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        private CoinWallet EnsureWallet(UserAccount user)
        {
            if (user.CoinWallet != null)
            {
                return user.CoinWallet;
            }

            var wallet = new CoinWallet
            {
                UserId = user.Id,
                Balance = 0,
                UpdatedAt = DateTime.Now
            };

            user.CoinWallet = wallet;
            _context.CoinWallets.Add(wallet);
            return wallet;
        }

        private static (int DamageBonus, double CooldownReduction) GetSelectedWeaponUpgradeBonuses(UserAccount user)
        {
            if (user.SelectedWeaponId == null)
            {
                return (0, 0);
            }

            var matchingUpgrades = user.PurchasedWeaponUpgrades
                .Where(pwu => pwu.WeaponUpgrade?.WeaponId == user.SelectedWeaponId)
                .Select(pwu => pwu.WeaponUpgrade)
                .Where(upgrade => upgrade != null)
                .ToList();

            return (
                matchingUpgrades.Sum(upgrade => upgrade!.DamageBonus),
                matchingUpgrades.Sum(upgrade => upgrade!.CooldownReduction));
        }

        private async Task<ShopViewModel> BuildShopViewModelAsync(UserAccount? user)
        {
            var userId = user?.Id ?? 0;
            var userLevel = user is null ? 1 : await GetUserLevelAsync(user);
            var userTotalScore = user?.Statistics?.TotalScore ?? 0;

            var favoriteWeaponIds = user is null
                ? new List<int>()
                : await _context.FavoriteWeapons
                    .Where(f => f.UserId == userId)
                    .Select(f => f.WeaponId)
                    .ToListAsync();

            return new ShopViewModel
            {
                UserLevel = userLevel,
                UserTotalScore = userTotalScore,
                CurrentLevelThreshold = _levelService.GetTotalScoreThresholdForLevel(userLevel),
                NextLevelThreshold = _levelService.GetNextLevelTotalScoreThreshold(userLevel),
                CoinBalance = user?.CoinWallet?.Balance ?? 0,
                SelectedWeaponId = user?.SelectedWeaponId,
                FavoriteWeaponIds = favoriteWeaponIds,
                PurchasedWeaponIds = user?.PurchasedWeapons.Select(pw => pw.WeaponId).Append(LevelService.StarterWeaponId).Distinct().ToList()
                    ?? new List<int> { LevelService.StarterWeaponId },
                PurchasedUpgradeIds = user?.PurchasedWeaponUpgrades.Select(pwu => pwu.WeaponUpgradeId).ToList()
                    ?? new List<int>(),
                OwnedPotionQuantities = user?.PurchasedPotions.ToDictionary(pp => pp.PotionId, pp => pp.Quantity)
                    ?? new Dictionary<int, int>(),
                Weapons = await _context.Weapons.OrderBy(w => w.Id).ToListAsync(),
                Potions = await _context.Potions.OrderBy(p => p.Price).ToListAsync(),
                WeaponUpgrades = await _context.WeaponUpgrades
                    .Include(wu => wu.Weapon)
                    .OrderBy(wu => wu.Price)
                    .ToListAsync()
            };
        }

        private async Task<List<SelectListItem>> LoadCountriesAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var countries = await httpClient.GetFromJsonAsync<List<CountryApiResponse>>("https://restcountries.com/v3.1/all?fields=name");

                return countries?
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name?.Common))
                    .Select(x => x.Name.Common)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    })
                    .ToList()
                    ?? new List<SelectListItem>();
            }
            catch
            {
                return new List<SelectListItem>();
            }
        }

        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }

        public async Task<IActionResult> Registration()
        {
            ViewBag.Countries = await LoadCountriesAsync();
            return View(new RegistrationViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            ViewBag.Countries = await LoadCountriesAsync();

            if (ModelState.IsValid)
            {
                UserAccount account = new UserAccount();
                account.Email = model.Email;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Country = model.Country;
                account.UserName = model.UserName;
                account.Password = model.Password;
                try
                {
                    _context.UserAccounts.Add(account);
                    _context.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered succesfully! You may now log in.";
                    return View(new RegistrationViewModel());
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the user account. Please try again.");
                    return View(model);
                }
            }
            return View(model);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Shop()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var user = await GetCurrentUserForShopAsync(userName);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = await BuildShopViewModelAsync(user);
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Jeśli użytkownik ma już ciasteczko (jest zalogowany), omiń formularz!
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SelectWeapon", "Account");
            }

            // W przeciwnym razie pokaż mu formularz
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model == null)
            {
                return View();
            }

            if (ModelState.IsValid)
            {
                var usernameOrEmail = model.UserNameOrEmail?.Trim();
                var password = model.Password;

                var user = await _context.UserAccounts
                    .FirstOrDefaultAsync(x => (x.UserName == usernameOrEmail || x.Email == usernameOrEmail)
                        && x.Password == password);

                if (user != null)
                {
                    // Successful login logic 
                    var clamis = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Name, user.Email),
               new Claim("Name",user.UserName),
               new Claim(ClaimTypes.Role, user.Role.ToString())
            };
                    var claimsidentity = new ClaimsIdentity(clamis, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsidentity), new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)    // 30 dni
                        : DateTimeOffset.UtcNow.AddHours(8)    // sesja robocza
                    });

                    return RedirectToAction("SelectWeapon", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username/email or password. Please try again.");
                }
            }
            // Return view with model to preserve entered username/email
            return View(model);
        }

        //changed login
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Pobranie nazwy użytkownika z Claimu (tak jak w SelectWeapon)
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Weryfikacja obecnego hasła 
            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Podane obecne hasło jest niepoprawne.");
                return View(model);
            }

            // Przypisanie nowego hasła i zapis w bazie danych
            user.Password = model.NewPassword;
            await _context.SaveChangesAsync();

            // Przekazanie komunikatu o sukcesie
            TempData["Message"] = "Twoje hasło zostało pomyślnie zmienione.";
            return RedirectToAction("SecurePage");
        }


        [HttpPost]
        public async Task<IActionResult> SelectWeapon(int weaponId)
        {
            // 1. Pobierz ID zalogowanego użytkownika (używamy claimu "Name" który przechowuje UserName)
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                // brak zalogowanego użytkownika -> przekieruj do logowania
                return RedirectToAction("Login");
            }

            var user = await _context.UserAccounts
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null)
            {
                var level = await GetUserLevelAsync(user);
                if (!_levelService.IsWeaponUnlocked(weaponId, level))
                {
                    TempData["WeaponSelectError"] = "Ta broń jest zablokowana. Zdobądź wyższy level, aby ją odblokować.";
                    return RedirectToAction("SelectWeapon");
                }

                if (!await HasPurchasedWeaponAsync(user.Id, weaponId))
                {
                    TempData["WeaponSelectError"] = "Najpierw kup tę broń za monety, a dopiero potem możesz ją wybrać.";
                    return RedirectToAction("SelectWeapon");
                }

                user.SelectedWeaponId = weaponId;

                // ZAPIS
                await _context.SaveChangesAsync();

                // Przekierowanie na stronę z podsumowaniem
                return RedirectToAction("SecurePage");
            }

            return RedirectToAction("Login");
        }





        [Authorize]
        public async Task<IActionResult> SecurePage()
        {

            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            if (int.TryParse(userIdClaim, out var currentUserId))
            {
                var suspension = await _context.UserPenalties
                    .Where(p => p.UserId == currentUserId
                        && p.IsActive
                        && (p.Type == PenaltyType.Suspension1Day || p.Type == PenaltyType.Suspension7Days)
                        && p.ExpiresAt.HasValue
                        && p.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(p => p.AppliedAt)
                    .FirstOrDefaultAsync();

                if (suspension != null)
                    return RedirectToAction("Suspended");
            }

            // Pobieramy użytkownika z bazy RAZEM z jego bronią
            var user = await _context.UserAccounts
                .Include(u => u.SelectedWeapon) // To załaduje statystyki broni
                .Include(u => u.CoinWallet)
                .Include(u => u.PurchasedPotions)
                    .ThenInclude(pp => pp.Potion)
                .Include(u => u.PurchasedWeaponUpgrades)
                    .ThenInclude(pwu => pwu.WeaponUpgrade)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null || user.SelectedWeapon == null)
            {
                // Jeśli nie wybrał broni, wyślij go do wyboru
                return RedirectToAction("SelectWeapon");
            }

            var (damageBonus, cooldownReduction) = GetSelectedWeaponUpgradeBonuses(user);
            var effectiveDamage = (user.SelectedWeapon?.Damage ?? 2) + damageBonus;
            var effectiveCooldown = Math.Max(0.05, (user.SelectedWeapon?.Cooldown ?? 1.0) - cooldownReduction);

            ViewBag.SelectedWeaponDamageBonus = damageBonus;
            ViewBag.SelectedWeaponCooldownReduction = cooldownReduction;
            ViewBag.EffectiveWeaponDamage = effectiveDamage;
            ViewBag.EffectiveWeaponCooldown = effectiveCooldown;
            ViewBag.SelectedWeaponUpgrades = user.PurchasedWeaponUpgrades
                .Where(pwu => pwu.WeaponUpgrade?.WeaponId == user.SelectedWeaponId)
                .Select(pwu => pwu.WeaponUpgrade!)
                .OrderBy(upgrade => upgrade.Name)
                .ToList();
            ViewBag.AvailablePotions = user.PurchasedPotions
                .Where(pp => pp.Quantity > 0)
                .OrderBy(pp => pp.Potion.Price)
                .ToList();

            ViewBag.Name = userName;

            // Przekazujemy obiekt broni (Weapon) jako model do widoku
            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> Suspended()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var suspension = await _context.UserPenalties
                .Where(p => p.UserId == userId
                    && p.IsActive
                    && (p.Type == PenaltyType.Suspension1Day || p.Type == PenaltyType.Suspension7Days)
                    && p.ExpiresAt.HasValue
                    && p.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(p => p.AppliedAt)
                .FirstOrDefaultAsync();

            if (suspension == null)
                return RedirectToAction("SecurePage");

            return View(suspension);
        }

        [Authorize]
        public IActionResult RemoveAccount()
        {
            // bezpieczne odczytanie emaila z claimu (ClaimTypes.Name trzymamy email przy logowaniu)
            var userEmail = HttpContext.User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var user = _context.UserAccounts.FirstOrDefault(x => x.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveAccountConfirmed()
        {
            var userEmail = HttpContext.User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var user = _context.UserAccounts.FirstOrDefault(x => x.Email == userEmail);

            if (user != null)
            {
                var userHighScores = await _context.HighScores
                    .Where(h => h.UserId == user.Id)
                    .ToListAsync();

                if (userHighScores.Count > 0)
                {
                    _context.HighScores.RemoveRange(userHighScores);
                }

                _context.UserAccounts.Remove(user);
                await _context.SaveChangesAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["Message"] = "Your account has been successfully deleted.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Login");
        }
        [HttpGet]
        public async Task<IActionResult> SelectWeapon()
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            var userId = 0;
            var userLevel = 1;
            var userTotalScore = 0;

            if (!string.IsNullOrEmpty(userName))
            {
                var user = await _context.UserAccounts
                    .Include(u => u.Statistics)
                    .Include(u => u.Level)
                    .Include(u => u.CoinWallet)
                    .Include(u => u.PurchasedWeapons)
                    .FirstOrDefaultAsync(u => u.UserName == userName);

                if (user != null)
                {
                    userId = user.Id;
                    userTotalScore = user.Statistics?.TotalScore ?? 0;
                    userLevel = await GetUserLevelAsync(user);
                    ViewBag.CoinBalance = user.CoinWallet?.Balance ?? 0;
                    ViewBag.PurchasedWeaponIds = user.PurchasedWeapons.Select(pw => pw.WeaponId).Append(LevelService.StarterWeaponId).Distinct().ToList();
                    ViewBag.SelectedWeaponId = user.SelectedWeaponId;
                }
            }

            // Pobierz wszystkie bronie
            var allWeapons = await _context.Weapons.ToListAsync();

            // Pobierz ulubione bronie użytkownika
            var favoriteWeaponIds = await _context.FavoriteWeapons
                .Where(f => f.UserId == userId)
                .Select(f => f.WeaponId)
                .ToListAsync();

            ViewBag.FavoriteWeaponIds = favoriteWeaponIds;

            ViewBag.UserLevel = userLevel;
            ViewBag.UserTotalScore = userTotalScore;
            ViewBag.CurrentLevelThreshold = _levelService.GetTotalScoreThresholdForLevel(userLevel);
            ViewBag.NextLevelThreshold = _levelService.GetNextLevelTotalScoreThreshold(userLevel);
            ViewBag.CoinBalance ??= 0;
            ViewBag.PurchasedWeaponIds ??= new List<int> { LevelService.StarterWeaponId };
            ViewBag.SelectedWeaponId ??= null;

            return View(allWeapons);
        }
        [HttpPost]
        public async Task<IActionResult> SelectWeaponConfirmed(int weaponId)
        {
            // Pobieramy UserName z Claimów (tak jak w Login przypisałeś: new Claim("Name", user.UserName))
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.UserAccounts
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null)
            {
                var level = await GetUserLevelAsync(user);
                if (!_levelService.IsWeaponUnlocked(weaponId, level))
                {
                    TempData["WeaponSelectError"] = "Ta broń jest zablokowana. Zdobądź wyższy level, aby ją odblokować.";
                    return RedirectToAction("SelectWeapon");
                }

                if (!await HasPurchasedWeaponAsync(user.Id, weaponId))
                {
                    TempData["WeaponSelectError"] = "Najpierw kup tę broń za monety, a dopiero potem możesz ją wybrać.";
                    return RedirectToAction("SelectWeapon");
                }

                user.SelectedWeaponId = weaponId;
                await _context.SaveChangesAsync();

                // Opcjonalnie: sprawdź w debuggerze czy tu wchodzi
            }

            return RedirectToAction("SecurePage");
        }
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Musisz być zalogowany" });
            }

            var user = await _context.UserAccounts
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return Json(new { success = false, message = "Użytkownik nie znaleziony" });
            }

            var level = await GetUserLevelAsync(user);
            if (!_levelService.IsWeaponUnlocked(request.WeaponId, level))
            {
                // UI nie pozwala na faworyzowanie zablokowanych broni (brak serca),
                // ale zabezpieczamy endpoint także po stronie serwera.
                return Json(new { success = false, message = "Nie można dodać zablokowanej broni do ulubionych." });
            }

            if (!await HasPurchasedWeaponAsync(user.Id, request.WeaponId))
            {
                return Json(new { success = false, message = "Nie można dodać niekupionej broni do ulubionych." });
            }

            // WALIDACJA: Sprawdź czy broń istnieje
            var weaponExists = await _context.Weapons.AnyAsync(w => w.Id == request.WeaponId);
            if (!weaponExists)
            {
                return Json(new { success = false, message = $"Broń o ID {request.WeaponId} nie istnieje" });
            }

            var favorite = await _context.FavoriteWeapons
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.WeaponId == request.WeaponId);

            if (favorite != null)
            {
                // Usuń z ulubionych
                _context.FavoriteWeapons.Remove(favorite);
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = false });
            }
            else
            {
                // Dodaj do ulubionych
                _context.FavoriteWeapons.Add(new FavoriteWeapon
                {
                    UserId = user.Id,
                    WeaponId = request.WeaponId,
                    MarkedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Json(new { success = true, isFavorite = true });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyWeapon(int weaponId)
        {
            // Block demo users from buying weapons
            if (User.FindFirst("IsDemo")?.Value == "true")
            {
                TempData["WeaponSelectError"] = "Buying weapons is not available in demo mode.";
                return RedirectToAction("Shop");
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.UserAccounts
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .Include(u => u.CoinWallet)
                .Include(u => u.PurchasedWeapons)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var weapon = await _context.Weapons.FirstOrDefaultAsync(w => w.Id == weaponId);
            if (weapon == null)
            {
                TempData["WeaponSelectError"] = "Nie znaleziono wybranej broni.";
                return RedirectToAction("Shop");
            }

            var level = await GetUserLevelAsync(user);
            if (!_levelService.IsWeaponUnlocked(weaponId, level))
            {
                TempData["WeaponSelectError"] = "Najpierw osiągnij wymagany level, aby kupić tę broń.";
                return RedirectToAction("Shop");
            }

            if (await HasPurchasedWeaponAsync(user.Id, weaponId))
            {
                TempData["WeaponSelectError"] = "Ta broń została już kupiona.";
                return RedirectToAction("Shop");
            }

            var wallet = EnsureWallet(user);

            var price = _levelService.GetWeaponPrice(weapon.Id, weapon.Damage);
            if (wallet.Balance < price)
            {
                TempData["WeaponSelectError"] = $"Masz za mało monet. Potrzebujesz {price}, a masz {wallet.Balance}.";
                return RedirectToAction("Shop");
            }

            wallet.Balance -= price;
            wallet.UpdatedAt = DateTime.Now;

            _context.PurchasedWeapons.Add(new PurchasedWeapon
            {
                UserId = user.Id,
                WeaponId = weapon.Id,
                PricePaid = price,
                PurchasedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["WeaponPurchaseSuccess"] = $"Kupiono broń {weapon.Name} za {price} monet.";
            return RedirectToAction("Shop");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyPotion(int potionId)
        {
            if (User.FindFirst("IsDemo")?.Value == "true")
            {
                TempData["WeaponSelectError"] = "Zakup potek jest niedostępny w trybie demo.";
                return RedirectToAction("Shop");
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var user = await GetCurrentUserForShopAsync(userName);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var potion = await _context.Potions.FirstOrDefaultAsync(p => p.Id == potionId);
            if (potion == null)
            {
                TempData["WeaponSelectError"] = "Nie znaleziono wybranej potki.";
                return RedirectToAction("Shop");
            }

            var wallet = EnsureWallet(user);
            if (wallet.Balance < potion.Price)
            {
                TempData["WeaponSelectError"] = $"Masz za mało monet na potkę {potion.Name}.";
                return RedirectToAction("Shop");
            }

            wallet.Balance -= potion.Price;
            wallet.UpdatedAt = DateTime.Now;

            var purchasedPotion = user.PurchasedPotions.FirstOrDefault(pp => pp.PotionId == potion.Id);
            if (purchasedPotion == null)
            {
                _context.PurchasedPotions.Add(new PurchasedPotion
                {
                    UserId = user.Id,
                    PotionId = potion.Id,
                    Quantity = 1,
                    PricePaid = potion.Price,
                    PurchasedAt = DateTime.Now
                });
            }
            else
            {
                purchasedPotion.Quantity += 1;
                purchasedPotion.PricePaid += potion.Price;
                purchasedPotion.PurchasedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["WeaponPurchaseSuccess"] = $"Kupiono potkę {potion.Name} za {potion.Price} monet.";
            return RedirectToAction("Shop");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyWeaponUpgrade(int weaponUpgradeId)
        {
            if (User.FindFirst("IsDemo")?.Value == "true")
            {
                TempData["WeaponSelectError"] = "Zakup ulepszeń jest niedostępny w trybie demo.";
                return RedirectToAction("Shop");
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            var user = await GetCurrentUserForShopAsync(userName);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var upgrade = await _context.WeaponUpgrades
                .Include(wu => wu.Weapon)
                .FirstOrDefaultAsync(wu => wu.Id == weaponUpgradeId);

            if (upgrade == null)
            {
                TempData["WeaponSelectError"] = "Nie znaleziono wybranego ulepszenia.";
                return RedirectToAction("Shop");
            }

            if (!await HasPurchasedWeaponAsync(user.Id, upgrade.WeaponId))
            {
                TempData["WeaponSelectError"] = $"Najpierw kup broń {upgrade.Weapon.Name}, aby odblokować jej ulepszenia.";
                return RedirectToAction("Shop");
            }

            if (user.PurchasedWeaponUpgrades.Any(pwu => pwu.WeaponUpgradeId == upgrade.Id))
            {
                TempData["WeaponSelectError"] = "To ulepszenie zostało już kupione.";
                return RedirectToAction("Shop");
            }

            var wallet = EnsureWallet(user);
            if (wallet.Balance < upgrade.Price)
            {
                TempData["WeaponSelectError"] = $"Masz za mało monet na ulepszenie {upgrade.Name}.";
                return RedirectToAction("Shop");
            }

            wallet.Balance -= upgrade.Price;
            wallet.UpdatedAt = DateTime.Now;

            _context.PurchasedWeaponUpgrades.Add(new PurchasedWeaponUpgrade
            {
                UserId = user.Id,
                WeaponUpgradeId = upgrade.Id,
                PricePaid = upgrade.Price,
                PurchasedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["WeaponPurchaseSuccess"] = $"Kupiono ulepszenie {upgrade.Name} za {upgrade.Price} monet.";
            return RedirectToAction("Shop");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UsePotion([FromBody] UsePotionRequest request)
        {
            if (User.FindFirst("IsDemo")?.Value == "true")
            {
                return Json(new { success = false, message = "Potki są niedostępne w trybie demo." });
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Musisz być zalogowany." });
            }

            var user = await _context.UserAccounts
                .Include(u => u.PurchasedPotions)
                    .ThenInclude(pp => pp.Potion)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return Json(new { success = false, message = "Nie znaleziono użytkownika." });
            }

            var purchasedPotion = user.PurchasedPotions.FirstOrDefault(pp => pp.PotionId == request.PotionId && pp.Quantity > 0);
            if (purchasedPotion?.Potion == null)
            {
                return Json(new { success = false, message = "Nie masz tej potki na stanie." });
            }

            purchasedPotion.Quantity -= 1;
            if (purchasedPotion.Quantity <= 0)
            {
                _context.PurchasedPotions.Remove(purchasedPotion);
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                potionId = purchasedPotion.PotionId,
                potionName = purchasedPotion.Potion.Name,
                effectStrength = purchasedPotion.Potion.EffectStrength,
                durationInSeconds = purchasedPotion.Potion.DurationInSeconds,
                remainingQuantity = Math.Max(0, purchasedPotion.Quantity)
            });
        }

        // Dodaj tę klasę na końcu AccountController (przed zamykającym })
        public class FavoriteRequest
        {
            public int WeaponId { get; set; }
        }

        public class UsePotionRequest
        {
            public int PotionId { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile, string? returnUrl = null)
        {
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login");

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return RedirectToAction("Login");

            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["AvatarError"] = "Nie wybrano pliku.";
                return returnUrl == "account"
                    ? RedirectToAction("Index", "Home")
                    : RedirectToAction("SecurePage");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                TempData["AvatarError"] = "Dozwolone formaty: jpg, jpeg, png, gif.";
                return returnUrl == "account"
                    ? RedirectToAction("Index", "Home")
                    : RedirectToAction("SecurePage");
            }

            if (avatarFile.Length > 2 * 1024 * 1024)
            {
                TempData["AvatarError"] = "Plik jest za duży. Maksymalny rozmiar to 2 MB.";
                return returnUrl == "account"
                    ? RedirectToAction("Index", "Home")
                    : RedirectToAction("SecurePage");
            }

            // Usuń stary awatar, jeśli istnieje
            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"{user.Id}_{Guid.NewGuid()}{extension}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            user.AvatarPath = $"/avatars/{fileName}";
            await _context.SaveChangesAsync();

            TempData["AvatarSuccess"] = "Awatar zaktualizowany!";
            return returnUrl == "account"
                ? RedirectToAction("Index", "Home")
                : RedirectToAction("SecurePage");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewProfile(int id)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdStr, out int currentUserId) && currentUserId == id)
            {
                // Jeśli gracz próbuje zobaczyć swój własny profil, przekieruj do SecurePage
                return RedirectToAction("SecurePage");
            }

            var user = await _context.UserAccounts
                .Include(u => u.SelectedWeapon)
                .Include(u => u.Statistics)
                .Include(u => u.Level)
                .Include(u => u.Club)
                .Include(u => u.UserAchievements)
                    .ThenInclude(ua => ua.Achievement)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null || user.IsDemo)
            {
                TempData["Error"] = "Nie znaleziono gracza.";
                return RedirectToAction("Highscores", "GameScore");
            }

       
            var savedScores = await _context.SavedScores
                .Where(s => s.UserId == id)
                .OrderByDescending(s => s.AchievedAt)
                .ToListAsync();

            var level = user.Level?.CurrentLevel
                ?? _levelService.GetLevelFromTotalScore(user.Statistics?.TotalScore ?? 0);

            var vm = new PublicProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Country = user.Country ?? string.Empty,
                AvatarPath = user.AvatarPath,
                MemberSince = user.CreatedAt,
                WeaponName = user.SelectedWeapon?.Name ?? "Pięści",
                WeaponImageUrl = user.SelectedWeapon?.ImageUrl,
                WeaponDamage = user.SelectedWeapon?.Damage ?? 10,
                CurrentLevel = level,
                TotalGamesPlayed = user.Statistics?.TotalGamesPlayed ?? 0,
                TotalScore = user.Statistics?.TotalScore ?? 0,
                HighestScore = user.Statistics?.HighestScore ?? 0,
                TotalClicks = user.Statistics?.TotalClicks ?? 0,
                CurrentStreak = user.Statistics?.CurrentStreak ?? 0,
                LongestStreak = user.Statistics?.LongestStreak ?? 0,

                ClubName = user.Club?.Name,
                ClubId = user.ClubId,
                Achievements = user.UserAchievements
                    .Where(ua => ua.Achievement != null)
                    .Select(ua => new AchievementInfo
                    {
                        Name = ua.Achievement.Name,
                        Description = ua.Achievement.Description,
                        ImagePath = ua.Achievement.ImagePath,
                        UnlockedAt = ua.UnlockedAt
                    })
                    .OrderByDescending(a => a.UnlockedAt)
                    .ToList(),

                SavedScores = savedScores
            };

            return View(vm);
        }
    }
}