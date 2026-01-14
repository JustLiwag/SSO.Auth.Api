using Azure;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(string employeeID, string employeePass)
{
    var client = _httpClientFactory.CreateClient();

    var payload = new
    {
        username = employeeID,
        password = employeePass
    };

    var content = new StringContent(
        JsonSerializer.Serialize(payload),
        Encoding.UTF8,
        "application/json"
    );

    var response = await client.PostAsync(
        "https://localhost:5001/api/auth/login",
        content
    );

    if (!response.IsSuccessStatusCode)
    {
        ViewBag.ErrorMessage = "Invalid credentials or access denied.";
        return View("~/Views/Login3/Index.cshtml");
    }

    // OPTIONAL: only if you want the token later
    var json = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(json);
    var token = doc.RootElement.GetProperty("token").GetString();

    Response.Cookies.Append("access_token", token!, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict
    });

    return RedirectToAction("Index", "Dashboard");
}
