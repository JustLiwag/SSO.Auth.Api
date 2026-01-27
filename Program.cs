using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Duende.IdentityServer;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Identity;

var builder = WebApplication.CreateBuilder(args);

// ====== Database ======
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ====== MVC ======
builder.Services.AddControllersWithViews();

// ====== IdentityServer ======
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;

    // Set login/logout UI paths
    options.UserInteraction.LoginUrl = "/account/login";
    options.UserInteraction.LogoutUrl = "/account/logout";
})
.AddInMemoryClients(IdentityServerConfig.Clients)
.AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
.AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
.AddProfileService<UserProfileService>()
.AddDeveloperSigningCredential();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Must come before IdentityServer middleware
app.UseAuthentication();

app.UseIdentityServer();
app.UseAuthorization();

// Default route for MVC views
app.MapDefaultControllerRoute();

app.Run();
