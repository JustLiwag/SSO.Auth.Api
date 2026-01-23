using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SSO.Auth.Api.Data;
using System.Security.Claims;

namespace SSO.Auth.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl)
        {
            var user = _db.Users
                .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                ViewData["Error"] = "Invalid credentials";
                return View();
            }

            var claims = new List<Claim>
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("name", user.EmployeeId)
        };

            var identity = new ClaimsIdentity(claims, "password");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

            return Redirect(returnUrl);
        }
    }
}
