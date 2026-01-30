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

            var context = await _interaction.GetAuthorizationContextAsync(ReturnUrl);
            string clientApp = context?.Client?.ClientName
                               ?? context?.Client?.ClientId
                               ?? "UNKNOWN";

            // =========================
            // 1️⃣ FIND USER FIRST
            // =========================
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    Username = username,
                    Action = "LOGIN_FAILED",
                    Reason = "User not found",
                    ClientApp = clientApp,
                    Timestamp = DateTime.Now
                });
                await _db.SaveChangesAsync();

                ViewBag.ReturnUrl = ReturnUrl;
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // =========================
            // 2️⃣ PASSWORD CHECK
            // =========================
            if (user.PasswordHash != password)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    Username = user.Username,
                    Action = "LOGIN_FAILED",
                    Reason = "Invalid password",
                    ClientApp = clientApp,
                    Timestamp = DateTime.Now
                });
                await _db.SaveChangesAsync();

                ViewBag.ReturnUrl = ReturnUrl;
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            // =========================
            // 3️⃣ STATUS CHECK
            // =========================
            var userStatus = await _db.PersonnelDivisionDetails
                .FirstOrDefaultAsync(x => x.employee_id == user.EmployeeId);

            if (userStatus == null ||
                userStatus.division_name == "INACTIVE" ||
                userStatus.separation_date != null)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    Username = user.Username,
                    Action = "LOGIN_FAILED",
                    Reason = "Employee separated or inactive",
                    ClientApp = clientApp,
                    Timestamp = DateTime.Now
                });
                await _db.SaveChangesAsync();

                ViewBag.ReturnUrl = ReturnUrl;
                ViewBag.Error = "Employee is inactive or separated";
                return View();
            }

            // =========================
            // 4️⃣ IDENTITYSERVER LOGIN
            // =========================
            var identityUser = new IdentityServerUser(user.EmployeeId)
            {
                DisplayName = user.Username
            };

            await HttpContext.SignInAsync(identityUser);

            // =========================
            // 5️⃣ SUCCESS AUDIT
            // =========================
            _db.AuditLogs.Add(new AuditLog
            {
                Username = user.Username,
                Action = "LOGIN",
                Reason = "Successful login",
                ClientApp = clientApp,
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return Redirect(ReturnUrl);
        }



        // =========================
        // LOGOUT (IMPORTANT)
        // =========================
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            
            var logoutContext = await _interaction.GetLogoutContextAsync(logoutId);

            string clientApp =
                logoutContext?.ClientName
                ?? logoutContext?.ClientId
                ?? "UNKNOWN";
            var username = User?.FindFirst("sub")?.Value ?? "UNKNOWN";

            if (User?.Identity?.IsAuthenticated == true)
            {
                //await HttpContext.SignOutAsync();
                // Sign out of IdentityServer session
                //await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
            }

            await HttpContext.SignOutAsync();

            _db.AuditLogs.Add(new AuditLog
            {
                Username = username,
                Action = "LOGOUT",
                Reason = "User Sign Out",
                ClientApp = clientApp,
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();


            return Redirect(logoutContext?.PostLogoutRedirectUri ?? "/");
        }
    }
}
