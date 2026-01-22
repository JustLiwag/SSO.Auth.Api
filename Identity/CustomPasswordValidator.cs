using Duende.IdentityServer.Validation;
using Duende.IdentityServer.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;

namespace SSO.Auth.Api.Identity
{
    /// Custom implementation of IResourceOwnerPasswordValidator.
    /// This class validates credentials against the application's database and
    /// issues claims used by IdentityServer tokens.
    public class CustomPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly AppDbContext _context;

        /// Construct using DI-provided <see cref="AppDbContext"/>.
        public CustomPasswordValidator(AppDbContext context)
        {
            _context = context;
        }

        /// Validate the resource owner credentials.
        /// If validation succeeds, set ResourceOwnerPasswordValidationContext.Result
        /// with subject and claims. On failure, set an appropriate error result.
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // Step 1: Lookup user by username (case-sensitive if needed)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => EF.Functions.Collate(u.Username, "Latin1_General_CS_AS") == context.UserName);

            if (user == null || user.PasswordHash != context.Password)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Invalid credentials");
                return;
            }

            // Step 2: Lookup personnel for additional details (matches AuthController)
            var personnel = await _context.PersonnelDivisionDetails
                .FirstOrDefaultAsync(p => p.employee_id == user.EmployeeId.ToString());

            if (personnel == null)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Employee record not found");
                return;
            }

            // Check separation_date to ensure employee is active
            if (personnel.separation_date != null && personnel.separation_date <= DateTime.Today)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Inactive employee");
                return;
            }

            // Step 3: Create claims to include in issued tokens
            var claims = new List<Claim>
            {
                new Claim("employee_no", personnel.employee_id.ToString()),
                new Claim("full_name", $"{personnel.given_name} {personnel.surname}"),
                new Claim("division", personnel.division_name)
            };

            // Step 4: Return successful validation result
            context.Result = new GrantValidationResult(
                subject: user.UserId.ToString(),
                authenticationMethod: "password",
                claims: claims
            );
        }
    }
}
