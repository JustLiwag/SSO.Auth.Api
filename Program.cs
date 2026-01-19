using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.Identity; // Custom validator lives here

// Entry point for the SSO.Auth.Api application.
// This file wires up EF Core, IdentityServer and middleware.
// Keep comments to help the team understand the registration order.

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Database
// ===========================
// Register EF Core DbContext for the application. Connection string is
// read from appsettings.json under "DefaultConnection".
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===========================
// IdentityServer
// ===========================
// Configure IdentityServer with in-memory clients/scopes for development.
// Replace in-memory stores and developer signing credential in production.
builder.Services.AddIdentityServer(options =>
{
    // EmitStaticAudienceClaim makes token audiences stable between IdentityServer versions.
    options.EmitStaticAudienceClaim = true;
})
.AddInMemoryClients(IdentityServerConfig.Clients)
.AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
.AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
.AddDeveloperSigningCredential(); // ⚠️ replace with cert in prod

// ===========================
// Custom Validator
// ===========================
// Register the password validator implementation from SSO.Auth.Api.Identity.
// This class performs user validation for the Resource Owner Password grant.
// Ensure the class exists in that namespace (CustomPasswordValidator).
builder.Services.AddTransient<IResourceOwnerPasswordValidator, CustomPasswordValidator>();

// ===========================
// Authentication for APIs
// ===========================
// Configure bearer authentication so downstream APIs can accept tokens
// issued by this IdentityServer instance (used for testing Swagger access).
builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    // IdentityServer base address (issuer)
    options.Authority = "https://localhost:5001";
    // In this sample we don't validate audience to keep tokens generic.
    options.TokenValidationParameters.ValidateAudience = false;
});

// Authorization, controllers and Swagger
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger/OpenAPI metadata
builder.Services.AddSwaggerGen(); // Generates Swagger documents for controllers

var app = builder.Build();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    // Enable Swagger only in Development to avoid exposing docs in prod.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseIdentityServer();   // Adds IdentityServer endpoints (/connect/token, etc.)
app.UseAuthentication();   // Enables authentication middleware
app.UseAuthorization();    // Enables authorization middleware

app.MapControllers();
app.Run();
