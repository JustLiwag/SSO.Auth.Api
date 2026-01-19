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
        /// with subject and claims. On failure set an appropriate error result.

        /// Validation context containing username/password.
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            // Step 1: Lookup user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == context.UserName);

            if (user == null || user.PasswordHash != context.Password)
            {
                // Invalid username/password
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Invalid credentials");
                return;
            }

            // Step 2: Ensure associated employee is active (not separated)
            // Note: user.EmployeeId may be stored as string in model; parse safely.
            var employeeId = int.TryParse(user.EmployeeId, out var parsedId) ? parsedId : 0;
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null || employee.DateOfSeparation != null)
            {
                // Employee missing or separated -- disallow token issuance
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Inactive employee");
                return;
            }

            // Step 3: Create claims to include in issued tokens
            var claims = new List<Claim>
            {
                new Claim("employee_no", employee.EmployeeNo),
                new Claim("full_name", employee.FullName),
                new Claim("division", employee.Division)
            };

            // Successful validation: set the subject to the application user id
            context.Result = new GrantValidationResult(
                subject: user.UserId.ToString(),
                authenticationMethod: "password",
                claims: claims
            );
        }
    }
}
