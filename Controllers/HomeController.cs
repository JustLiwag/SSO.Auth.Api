using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace SSO.Auth.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [HttpGet("/home/error")]
        public async Task<IActionResult> Error(string errorId)
        {
            var message = await _interaction.GetErrorContextAsync(errorId);

            if (message == null)
                return Content("No error details found.");

            return Content($@"
ERROR: {message.Error}

DESCRIPTION:
{message.ErrorDescription}
");
        }
    }
}
