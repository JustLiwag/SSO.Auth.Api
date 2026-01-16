using Duende.IdentityServer.Validation;
using Duende.IdentityServer.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;

namespace SSO.Auth.Api.Identity
{

    public class CustomPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly AppDbContext _context;

        public CustomPasswordValidator(AppDbContext context)
        {
            _context = context;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == context.UserName);

            if (user == null || user.PasswordHash != context.Password)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Invalid credentials");
                return;
            }

            var employeeId = int.TryParse(user.EmployeeId, out var parsedId) ? parsedId : 0;
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null || employee.DateOfSeparation != null)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Inactive employee");
                return;
            }

            var claims = new List<Claim>
            {
                new Claim("employee_no", employee.EmployeeNo),
                new Claim("full_name", employee.FullName),
                new Claim("division", employee.Division)
            };

            context.Result = new GrantValidationResult(
                subject: user.UserId.ToString(),
                authenticationMethod: "password",
                claims: claims
            );
        }
    }

}
