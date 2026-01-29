using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
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
        private readonly IIdentityServerInteractionService _interaction;
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db, IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
            _db = db;
        }

        // =========================
        // LOGIN
        // =========================
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !_interaction.IsValidReturnUrl(returnUrl))
            {
                return Redirect("~/");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string ReturnUrl)
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl) || !_interaction.IsValidReturnUrl(ReturnUrl))
            {
                ModelState.AddModelError("", "Invalid return URL.");
                ViewBag.ReturnUrl = ReturnUrl;
                return View();
            }

            // =========================
            // YOUR EXISTING VALIDATION
            // =========================
            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user == null || user.PasswordHash != password)
            {
                ViewBag.ReturnUrl = ReturnUrl;
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

            await HttpContext.SignInAsync(identityUser);

            return Redirect(ReturnUrl);
        }




        // =========================
        // LOGOUT (IMPORTANT)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var logoutContext = await _interaction.GetLogoutContextAsync(logoutId);

            if (User?.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync();
            }

            return Redirect(logoutContext?.PostLogoutRedirectUri ?? "/");
        }
    }
}
