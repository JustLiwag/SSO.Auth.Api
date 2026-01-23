using SSO.Auth.Api.Data;
using SSO.Auth.Api.Identity;

var builder = WebApplication.CreateBuilder(args);

// =======================
// DATABASE
// =======================
builder.Services.AddDbContext<AppDbContext>();

// =======================
// MVC (for login UI)
// =======================
builder.Services.AddControllersWithViews();

// =======================
// IDENTITY SERVER
// =======================
builder.Services
    .AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;
    })
    .AddInMemoryClients(IdentityServerConfig.Clients)
    .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
    .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
    .AddProfileService<UserProfileService>() // adds claims
    .AddDeveloperSigningCredential();

var app = builder.Build();

// =======================
// MIDDLEWARE
// =======================
app.UseStaticFiles();
app.MapDefaultControllerRoute();
app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
