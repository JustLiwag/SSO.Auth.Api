using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.DTOs;
using SSO.Auth.Api.Models;
using System.Text.Json;


/// Authentication API endpoints used by clients.
/// This controller contains a lightweight login endpoint (used for demo/testing)
/// and an authenticated /me endpoint that returns current claims.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;


    /// Constructor receives DbContext via dependency injection.
    public AuthController(AppDbContext context)
    {
        _context = context;
    }


    /// Authenticate a user using username/password.
    /// This endpoint performs application-level checks (personnel record, separation date, active flag)
    /// and logs audit entries for successes and failures.

    /// Login request DTO (username/password)
    /// LoginResponse on success, Unauthorized on failure.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            // Use case-sensitive collation when comparing usernames to preserve exact matches.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => EF.Functions.Collate(u.Username, "Latin1_General_CS_AS") == request.Username);

            if (user == null || user.PasswordHash != request.Password)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Invalid credentials");
                return Unauthorized("Invalid credentials");
            }

            // Lookup the personnel view for additional data (this maps to a DB view)
            var personnel = await _context.PersonnelDivisionDetails
                .FirstOrDefaultAsync(p => p.employee_id == user.EmployeeId.ToString());

            if (personnel == null)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Personnel record missing");
                return Unauthorized("Employee record not found");
            }

            // Check separation_date (if present) to ensure employee is active
            if (personnel.separation_date != null &&
                personnel.separation_date <= DateTime.Today)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Employee separated");
                return Unauthorized("Employee is no longer active");
            }

            // Also check the Users.IsActive flag as an alternative source of truth
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (!userEntity.IsActive)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "User is inactive");
                return Unauthorized("Employee is no longer active");
            }

            // Optional: check attendance/time-in for today (commented out in current flow)
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            bool hasTimedIn = await _context.Attendance.AnyAsync(a =>
                a.EmployeeId.ToString() == user.EmployeeId &&
                a.TimeIn.HasValue &&
                a.TimeIn.Value >= today &&
                a.TimeIn.Value < tomorrow
            );

            //if (!hasTimedIn)
            //{
            //    LogAudit(request.Username, "LOGIN_FAILED", "No time-in today");
            //    return Unauthorized("No time-in record found");
            //}

            // Audit successful login
            LogAudit(request.Username, "LOGIN_SUCCESS", "Login successful");

            // =========================================
            // New: Get real token from IdentityServer
            // =========================================
            var tokenEndpoint = $"{Request.Scheme}://{Request.Host}/connect/token";
            using var client = new HttpClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "hris_client",
                ["client_secret"] = "hris_secret",
                ["username"] = request.Username,
                ["password"] = request.Password,
                ["scope"] = "openid profile sso_api"
            });

            var response = await client.PostAsync(tokenEndpoint, content);
            var json = await response.Content.ReadAsStringAsync();

            // Return the IdentityServer token response along with your existing user info
            return Ok(new
            {
                TokenResponse = JsonSerializer.Deserialize<JsonElement>(json),
                EmployeeId = personnel.employee_id,
                EmployeeNo = personnel.employee_id.ToString(),
                FullName = $"{personnel.given_name} {personnel.surname}",
                Division = personnel.division_name
            });
        }
        catch (Exception ex)
        {
            // Return 500 for unexpected errors. Consider capturing/logging exceptions to monitoring.
            return StatusCode(500, ex.Message);
        }
    }


    /// Persist a simple audit log entry to the database.
    /// Keep this synchronous for simplicity; can be offloaded to a background queue later.
    private void LogAudit(string username, string action, string reason)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Username = username,
            Action = action,
            Reason = reason,
            Timestamp = DateTime.Now
        });
        _context.SaveChanges();
    }


    /// Returns the claims for the currently authenticated user.
    /// IdentityServer will populate claims when tokens are issued.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        // Claims will be populated by IdentityServer later.
        // This endpoint is intentionally simple: it echoes the claims.
        return Ok(User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        }));
    }
}
