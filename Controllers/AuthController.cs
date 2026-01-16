using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.DTOs;
using SSO.Auth.Api.Models;
using SSO.Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                EF.Functions.Collate(u.Username, "Latin1_General_CS_AS") == request.Username);

        // USER NOT FOUND
        if (user == null)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "INVALID_CREDENTIALS");
            return Unauthorized("Invalid credentials");
        }

        // 🔒 LOCKOUT CHECK
        if (user.LockoutUntil != null && user.LockoutUntil > DateTime.Now)
        {
            var remainingMinutes =
                (int)(user.LockoutUntil.Value - DateTime.Now).TotalMinutes;

            LogAudit(request.Username, "LOGIN_FAILED", "ACCOUNT_LOCKED");
            return Unauthorized($"Account locked. Try again in {remainingMinutes} minute(s).");
        }

        // PASSWORD CHECK (unchanged logic)
        bool validPassword = user.PasswordHash == request.Password;

        if (!validPassword)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "INVALID_CREDENTIALS");

            // 🔍 CHECK LAST 5 ATTEMPTS
            var lastAttempts = await _context.AuditLogs
                .Where(a =>
                    a.Username == request.Username &&
                    (a.Action == "LOGIN_FAILED" || a.Action == "LOGIN_SUCCESS"))
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .ToListAsync();

            bool shouldLock =
                lastAttempts.Count == 5 &&
                lastAttempts.All(a =>
                    a.Action == "LOGIN_FAILED" &&
                    a.Reason == "INVALID_CREDENTIALS");

            if (shouldLock)
            {
                user.LockoutUntil = DateTime.Now.AddMinutes(30);
                await _context.SaveChangesAsync();

                return Unauthorized("Account locked for 30 minutes.");
            }

            return Unauthorized("Invalid credentials");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);

        if (employee.DateOfSeparation != null && employee.DateOfSeparation <= DateTime.Today)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "EMPLOYEE_SEPARATED");
            return Unauthorized("Employee is separated already");
        }

        // IsActive check (unchanged)
        var userEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == user.UserId);

        if (!userEntity.IsActive)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "USER_INACTIVE");
            return Unauthorized("Employee is no longer active");
        }

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        bool hasTimedIn = await _context.Attendance.AnyAsync(a =>
            a.EmployeeId == employee.EmployeeId &&
            a.TimeIn.HasValue &&
            a.TimeIn.Value >= today &&
            a.TimeIn.Value < tomorrow
        );

        if (!hasTimedIn)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "NO_TIME_IN");
            return Unauthorized("No time-in record found");
        }

        // ✅ SUCCESS — reset lock
        user.LockoutUntil = null;
        await _context.SaveChangesAsync();

        LogAudit(request.Username, "LOGIN_SUCCESS", "Successful Login");

        var token = _jwtService.GenerateToken(user, employee);

        return Ok(new
        {
            token,
            employee.EmployeeNo,
            employee.FullName,
            employee.Division
        });
    }

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

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        }));
    }
}
