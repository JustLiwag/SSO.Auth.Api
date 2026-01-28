using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Models;

namespace SSO.Auth.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IIdentityServerInteractionService _interactionService;

        // ✅ FIXED CONSTRUCTOR (THIS WAS THE ROOT CAUSE)
        public AccountController(
            AppDbContext db,
            IIdentityServerInteractionService interactionService
        )
        {
            _db = db;
            _interactionService = interactionService;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password,
            string returnUrl
        )
        {
            // 🔒 REQUIRED FOR OIDC
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return BadRequest("Missing returnUrl");
            }

            // =========================
            // YOUR EXISTING VALIDATION
            // =========================
            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user == null || user.PasswordHash != password)
            {
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // =========================
            // 🔑 IDENTITYSERVER SIGN-IN
            // =========================
            var identityUser = new IdentityServerUser(user.EmployeeId)
            {
                DisplayName = user.Username
            };

            await HttpContext.SignInAsync(identityUser);

            // =========================
            // AUDIT LOG (KEPT)
            // =========================
            _db.AuditLogs.Add(new AuditLog
            {
                Username = user.Username,
                Action = "LoginSuccess",
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // 🔁 RETURN TO CLIENT (VAMS)
            return Redirect(returnUrl);
        }

        // =========================
        // POST: /Account/Logout
        // (Manual logout from UI)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                IdentityServerConstants.DefaultCookieAuthenticationScheme
            );

            return Redirect("~/");
        }

        // =========================
        // GET: /Account/Logout
        // (OIDC RP-Initiated Logout)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var logoutContext =
                await _interactionService.GetLogoutContextAsync(logoutId);

            // 🔐 Sign out IdentityServer cookie
            await HttpContext.SignOutAsync(
                IdentityServerConstants.DefaultCookieAuthenticationScheme
            );

            // 🔁 Redirect back to client
            if (!string.IsNullOrEmpty(logoutContext?.PostLogoutRedirectUri))
            {
                return Redirect(logoutContext.PostLogoutRedirectUri);
            }

            return Redirect("~/");
        }
    }
}
