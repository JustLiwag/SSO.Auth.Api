using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SSO.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TokenTestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenTestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Temporary endpoint to test IdentityServer token issuance via Swagger.
        /// </summary>
        /// <param name="username">User login</param>
        /// <param name="password">User password</param>
        /// <returns>Access token JSON</returns>
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

            return Content(json, "application/json");
        }
    }
}
