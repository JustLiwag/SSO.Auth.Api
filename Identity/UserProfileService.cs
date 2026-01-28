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

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var employeeNo = context.Subject.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(employeeNo))
            return;

        var user = _db.PersonnelDivisionDetails
            .FirstOrDefault(x => x.employee_id == employeeNo);

        if (user == null)
            return;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.given_name + " " + user.surname ?? ""),
            new Claim("division", user.division_name ?? "")
        };

        context.IssuedClaims.AddRange(claims);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = true;
        return Task.CompletedTask;
    }
}
