using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Models;
using System.Security.Claims;

namespace SSO.Auth.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        [HttpGet("/account/login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/account/login")]
        public async Task<IActionResult> LoginPost(string username, string password, string returnUrl)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                ViewData["Error"] = "Invalid credentials";
                ViewData["ReturnUrl"] = returnUrl;
                return View("Login");
            }

            var claims = new List<Claim>
            {
                new Claim("sub", user.UserId.ToString()),
                new Claim("name", user.EmployeeId)
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                IdentityServerConstants.DefaultCookieAuthenticationScheme));

            await HttpContext.SignInAsync(
                IdentityServerConstants.DefaultCookieAuthenticationScheme,
                principal);

            return Redirect(returnUrl ?? "/");
        }
    }
}
