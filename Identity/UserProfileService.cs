using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using SSO.Auth.Api.Data;
using System.Security.Claims;

namespace SSO.Auth.Api.Identity;

public class UserProfileService : IProfileService
{
    private readonly AppDbContext _db;

    public UserProfileService(AppDbContext db)
    {
        _db = db;
    }

    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var userId = context.Subject.FindFirst("sub")?.Value;

        var claims = new List<Claim>
        {
            new Claim("employee_id", userId ?? "")
        };

        context.IssuedClaims = claims;
        return Task.CompletedTask;
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}
