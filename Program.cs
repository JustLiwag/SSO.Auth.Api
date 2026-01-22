using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Identity; // Custom validator lives here
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Database
// ===========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===========================
// Razor Pages (for login/logout)
// ===========================
builder.Services.AddRazorPages();

// ===========================
// IdentityServer
// ===========================
builder.Services.AddIdentityServer(options =>
{
    options.EmitStaticAudienceClaim = true;

    // Set login/logout pages
    options.UserInteraction.LoginUrl = "/Account/Login";
    options.UserInteraction.LogoutUrl = "/Account/Logout";
})
.AddInMemoryClients(IdentityServerConfig.Clients)
.AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
.AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
.AddDeveloperSigningCredential(); // ⚠️ Replace with a real certificate in production

// ===========================
// Custom Validator
// ===========================
builder.Services.AddTransient<IResourceOwnerPasswordValidator, CustomPasswordValidator>();

// ===========================
// Authentication for APIs
// ===========================
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // IdentityServer URL
        options.TokenValidationParameters.ValidateAudience = false;
    });

// ===========================
// Authorization, Controllers, Swagger
// ===========================
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===========================
// Middleware pipeline
// ===========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

// Razor Pages endpoints
app.MapRazorPages(); // must come before IdentityServer middleware

app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
