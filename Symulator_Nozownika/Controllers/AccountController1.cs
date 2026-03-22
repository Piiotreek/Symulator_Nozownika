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
                       new Claim("Name",user.FirstName),
                       new Claim(ClaimTypes.Role, "User")
                    }; 
                    var claimsidentity = new ClaimsIdentity(clamis, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsidentity));

                    return RedirectToAction("SecurePage", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username/email or password. Please try again.");
                }
            }
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
    }
}
