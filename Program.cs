using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Services;
using SSO.Auth.Api.Identity;


var builder = WebApplication.CreateBuilder(args);

// ===========================
// Database
// ===========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===========================
// IdentityServer
// ===========================
builder.Services.AddIdentityServer(options =>
{
    options.EmitStaticAudienceClaim = true;
})
.AddInMemoryClients(IdentityServerConfig.Clients)
.AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
.AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
.AddDeveloperSigningCredential(); // ⚠️ replace with cert in prod

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
    options.Authority = "https://localhost:5001";
    options.TokenValidationParameters.ValidateAudience = false;
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // ✅ Required for Swagger
builder.Services.AddSwaggerGen(); // ✅ Required for Swagger

var app = builder.Build();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // ✅ Swagger JSON
    app.UseSwaggerUI(); // ✅ Swagger UI page
}

app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
