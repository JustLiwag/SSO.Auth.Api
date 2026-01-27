using Duende.IdentityModel;
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
            ViewData["ReturnUrl"] = returnUrl;
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
            string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // -------------------------
            // Basic validation
            // -------------------------
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            // -------------------------
            // 1. Validate credentials
            // -------------------------
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || user.PasswordHash != password)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            // -------------------------
            // 2. Check employment status via VIEW
            // -------------------------
            var personnel = await _db.PersonnelDivisionDetails
                .FirstOrDefaultAsync(p => p.employee_id == user.EmployeeId);

            if (personnel == null)
            {
                ModelState.AddModelError("", "Personnel record not found.");
                return View();
            }

            if (personnel.separation_date != null ||
                personnel.division_name.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Your account is inactive or separated.");
                return View();
            }

            // -------------------------
            // 3. Create claims
            // -------------------------
            var claims = new List<Claim>
            {
                // REQUIRED by Duende
                new Claim(JwtClaimTypes.Subject, user.EmployeeId),

                // Optional but recommended
                new Claim(JwtClaimTypes.PreferredUserName, user.Username),
                new Claim(JwtClaimTypes.Name, user.Username),

                // Custom claim
                new Claim("division", personnel.division_name)
            };


            var identity = new ClaimsIdentity(
                claims,
                IdentityServerConstants.DefaultCookieAuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            // -------------------------
            // 4. Sign in to IdentityServer
            // -------------------------
            await HttpContext.SignInAsync(
                IdentityServerConstants.DefaultCookieAuthenticationScheme,
                principal
            );

            // -------------------------
            // 5. Audit log (SUCCESS)
            // -------------------------
            _db.AuditLogs.Add(new AuditLog
            {
                Username = user.Username,
                Action = "LoginSuccess",
                Reason = "Valid login",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // -------------------------
            // 6. Redirect back to client
            // -------------------------
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
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
