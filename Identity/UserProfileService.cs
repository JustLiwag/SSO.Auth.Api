namespace SSO.Auth.Api.Identity
{
    using Duende.IdentityServer.Models;
    using Duende.IdentityServer.Services;
    using SSO.Auth.Api.Data;
    using System.Security.Claims;

    public class UserProfileService : IProfileService
    {
        private readonly AppDbContext _db;

        public UserProfileService(AppDbContext db)
        {
            _db = db;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var userId = context.Subject.FindFirst("sub")?.Value;

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;

            context.IssuedClaims.AddRange(new[]
            {
            new Claim("employee_no", user.EmployeeId),
            new Claim("full_name", user.EmployeeId),
            new Claim("division", user.EmployeeId)
        });
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
        }
    }

}
