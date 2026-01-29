using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SSO.Auth.Api.Models;

namespace SSO.Auth.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        // =========================
        // LOGIN
        // =========================
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // If no returnUrl, send user back to client via authorize
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect("~/");
            }

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            var vm = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.ReturnUrl) ||
                !_interaction.IsValidReturnUrl(model.ReturnUrl))
            {
                ModelState.AddModelError("", "Invalid return URL.");
                return View(model);
            }

            // TODO: replace with real user validation
            if (model.Username != "admin" || model.Password != "password")
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            var user = new IdentityServerUser(model.Username)
            {
                DisplayName = model.Username
            };

            await HttpContext.SignInAsync(user);

            return Redirect(model.ReturnUrl);
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
