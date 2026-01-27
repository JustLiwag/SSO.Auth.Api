using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Users
    .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account is inactive.");
                return View(model);
            }

            if (user.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Invalid password");
                return View(model);
            }


            // 2️⃣ Get employee info from the view
            var employee = await _db.PersonnelDivisionView
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.employee_id == user.EmployeeId);

            // 3️⃣ Block separated users
            if (employee?.separation_date != null)
            {
                ModelState.AddModelError("", "Your account is no longer active.");
                return View(model);
            }

            // 4️⃣ Map claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("employee_id", user.EmployeeId)
    };

            if (employee != null)
            {
                claims.Add(new Claim("surname", employee.surname));
                claims.Add(new Claim("given_name", employee.given_name));
                claims.Add(new Claim("division_name", employee.division_name));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            var principal = new ClaimsPrincipal(identity);

            // 5️⃣ Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // 6️⃣ Audit Log (match your table structure)
            _db.AuditLogs.Add(new AuditLog
            {
                Username = user.Username,
                Action = "Login",
                Reason = "Successful login",
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // 7️⃣ Redirect
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
