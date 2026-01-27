using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using SSO.Auth.Api.Data;
using System.Security.Claims;

public class UserProfileService : IProfileService
{
    private readonly AppDbContext _db;
    public UserProfileService(AppDbContext db) => _db = db;

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var sub = context.Subject.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub)) return;

        var user = await _db.Users.FindAsync(int.Parse(sub));
        if (user == null) return;

        var claims = new List<Claim>
        {
            new Claim("employee_no", user.EmployeeId),
            new Claim("name", user.EmployeeId)
            // add more if you read from PersonnelDivisionDetails later
        };
        context.IssuedClaims.AddRange(claims);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}
