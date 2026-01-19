using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SSO.Auth.Api.Controllers
{

    /// Temporary controller to allow testing IdentityServer token issuance via Swagger/UI.
    /// Intended for manual testing only—remove or secure in production.
    [ApiController]
    [Route("api/test")]
    public class TokenTestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenTestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        /// Request an access token using Resource Owner Password grant.
        /// This method forwards form-encoded parameters to the IdentityServer token endpoint.

        /// <param name="username">User login</param>
        /// <param name="password">User password</param>
        /// <returns>Raw JSON response returned by IdentityServer token endpoint</returns>
        [HttpPost("get-token")]
        public async Task<IActionResult> GetToken([FromForm] string username, [FromForm] string password)
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5001/connect/token");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "hris_client",
                ["client_secret"] = "hris_secret",
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid profile sso_api"
            });

            request.Content = content;

            var response = await client.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            // Return token response as JSON to simplify testing via Swagger.
            return Content(json, "application/json");
        }
    }
}
