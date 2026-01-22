using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace SSO.Auth.Api.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;

        public LoginModel(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // TODO: Replace this with real user validation (DB check)
            if (Username == "admin" && Password == "password")
            {
                // ✅ Create IdentityServerUser
                var user = new IdentityServerUser(Username)
                {
                    DisplayName = Username,
                    // Optional: add extra claims
                    AdditionalClaims = new List<Claim>
                    {
                        new Claim("role", "admin")
                    }
                };

                // ✅ Sign in the user
                await HttpContext.SignInAsync(user);

                // ✅ If return URL exists, redirect to it
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    return LocalRedirect(ReturnUrl);

                // Default redirect if no return URL
                return RedirectToPage("/Index");
            }

            // Invalid credentials
            ErrorMessage = "Invalid username or password";
            return Page();
        }
    }
}
