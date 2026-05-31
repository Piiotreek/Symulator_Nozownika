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
               new Claim(ClaimTypes.Role, "User")
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

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            // Pobieramy użytkownika z bazy RAZEM z jego bronią
            var user = await _context.UserAccounts
                .Include(u => u.SelectedWeapon) // To załaduje statystyki broni
                .Include(u => u.CoinWallet)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null || user.SelectedWeapon == null)
            {
                // Jeśli nie wybrał broni, wyślij go do wyboru
                return RedirectToAction("SelectWeapon");
            }

            ViewBag.Name = userName;

            // Przekazujemy obiekt broni (Weapon) jako model do widoku
            return View(user);
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
                return RedirectToAction("SelectWeapon");
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
                return RedirectToAction("SelectWeapon");
            }

            var level = await GetUserLevelAsync(user);
            if (!_levelService.IsWeaponUnlocked(weaponId, level))
            {
                TempData["WeaponSelectError"] = "Najpierw osiągnij wymagany level, aby kupić tę broń.";
                return RedirectToAction("SelectWeapon");
            }

            if (await HasPurchasedWeaponAsync(user.Id, weaponId))
            {
                TempData["WeaponSelectError"] = "Ta broń została już kupiona.";
                return RedirectToAction("SelectWeapon");
            }

            var wallet = user.CoinWallet;
            if (wallet == null)
            {
                wallet = new CoinWallet
                {
                    UserId = user.Id,
                    Balance = 0,
                    UpdatedAt = DateTime.Now
                };

                _context.CoinWallets.Add(wallet);
            }

            var price = _levelService.GetWeaponPrice(weapon.Id, weapon.Damage);
            if (wallet.Balance < price)
            {
                TempData["WeaponSelectError"] = $"Masz za mało monet. Potrzebujesz {price}, a masz {wallet.Balance}.";
                return RedirectToAction("SelectWeapon");
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
            return RedirectToAction("SelectWeapon");
        }

        // Dodaj tę klasę na końcu AccountController (przed zamykającym })
        public class FavoriteRequest
        {
            public int WeaponId { get; set; }
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
    }
}
