using FixedApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FixedApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username)
        {
            var uname = _context.Users.Include(u => u.Roles)
                .FirstOrDefault(u => u.Username == username);
            if (uname != null)
            {
                List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, uname.Id.ToString()),
                        new Claim(ClaimTypes.Name, uname.Username)
                    };
                foreach (var role in uname.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                }
                ClaimsIdentity identity = new ClaimsIdentity(claims,
                    "BacCookieAuth");
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("BacCookieAuth", principal);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Invalid username";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("BacCookieAuth");
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return Content("ERROR 403");
        }
    }
}
