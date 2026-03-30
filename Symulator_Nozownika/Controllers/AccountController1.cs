using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class AccountController1 : Controller
    {
        private readonly AppDbContext _context;

        public AccountController1(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tutaj można dodać logikę rejestracji użytkownika, np. zapis do bazy danych
                // Po udanej rejestracji można przekierować użytkownika na inną stronę, np. stronę logowania
                UserAccount account = new UserAccount();
                account.Email = model.Email;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.UserName = model.UserName;
                account.Password = model.Password;
                try
                {
                    _context.UserAccounts.Add(account);
                    _context.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered succesfully! You may now log in.";
                }
                catch (DbUpdateException ex)
                {

                    ModelState.AddModelError("", "An error occurred while saving the user account. Please try again.");
                    return View(model);
                }
                return View();

                //return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        public IActionResult Login()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.UserAccounts.Where(x => (x.UserName == model.UserNameOrEmail || x.Email == model.UserNameOrEmail)
                && x.Password == model.Password).FirstOrDefault();
                if (user != null)
                {
                    //Successful login logic 
                    var clamis = new List<Claim>
                    {
                       new Claim(ClaimTypes.Name, user.Email),
                       new Claim("Name",user.UserName),
                       new Claim(ClaimTypes.Role, "User")
                    };
                    var claimsidentity = new ClaimsIdentity(clamis, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsidentity));

                    return RedirectToAction("SecurePage", "AccountController1");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username/email or password. Please try again.");
                }
            }
            return View();
        }
        //changed login
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> SecurePage()
        {
            // Pobieramy nazwę użytkownika z Claimów (tak jak masz teraz)
            var userName = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login");
            }

            // Pobieramy użytkownika z bazy RAZEM z jego bronią
            var user = await _context.UserAccounts
                .Include(u => u.SelectedWeapon) // To załaduje statystyki broni
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null || user.SelectedWeapon == null)
            {
                // Jeśli nie wybrał broni, wyślij go do wyboru
                return RedirectToAction("SelectWeapon");
            }

            ViewBag.Name = userName;

            // Przekazujemy obiekt broni (Weapon) jako model do widoku
            return View(user.SelectedWeapon);
        }

        [Authorize]
        public IActionResult RemoveAccount()
        {
            var userEmail = HttpContext.User.Identity.Name;
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
            var userEmail = HttpContext.User.Identity.Name;
            var user = _context.UserAccounts.FirstOrDefault(x => x.Email == userEmail);

            if (user != null)
            {
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
            // Pobieramy listę wszystkich noży/toporów z bazy danych
            var allWeapons = await _context.Weapons.ToListAsync();
            return View(allWeapons);
        }
        [HttpPost]
        public async Task<IActionResult> SelectWeaponConfirmed(int weaponId)
        {
            // Zakładając, że logujesz gracza na sesji lub po Username
            var userName = HttpContext.Session.GetString("UserName"); // Przykładowe pobieranie usera z sesji

            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login");

            var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null)
            {
                user.SelectedWeaponId = weaponId;
                await _context.SaveChangesAsync(); // To zapisuje zmianę w bazie danych
            }

            return RedirectToAction("SecurePage");
        }
    }
}
