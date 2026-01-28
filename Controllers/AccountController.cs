
using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Models;


namespace SSO.Auth.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // GET: /Account/Login
        // =========================
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // =========================
        // POST: /Account/Login
        // =========================
        [HttpPost]
        public async Task<IActionResult> Login(
            string username,
            string password,
            string returnUrl
)
        {
            // 🔍 SAFETY CHECK (optional but recommended)
            if (string.IsNullOrEmpty(returnUrl))
            {
                return BadRequest("Missing returnUrl");
            }

            // ============================
            // YOUR EXISTING VALIDATIONS
            // ============================
            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user == null || user.PasswordHash != password)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            var identityUser = new IdentityServerUser(user.EmployeeId)
            {
                DisplayName = user.Username
            };

            await HttpContext.SignInAsync(identityUser);

            return Redirect(returnUrl);


            // ============================
            // AUDIT LOG (KEEP YOURS)
            // ============================
            _db.AuditLogs.Add(new AuditLog
            {
                Username = user.Username,
                Action = "LoginSuccess",
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // ============================
            // 🔥 DO NOT CHANGE THIS
            // ============================
            return Redirect(returnUrl);
        }



        // =========================
        // POST: /Account/Logout
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                IdentityServerConstants.DefaultCookieAuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }
    }
}
