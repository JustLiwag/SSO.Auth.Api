using Duende.IdentityServer;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityServer()
    .AddInMemoryClients(IdentityServerConfig.Clients)
    .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
    .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
    .AddProfileService<UserProfileService>();

builder.Services.AddAuthentication()
    .AddCookie();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
