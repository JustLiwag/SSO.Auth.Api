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


        if (user == null || user.PasswordHash != request.Password)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "Invalid credentials");
            return Unauthorized("Invalid credentials");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);

        if (employee.DateOfSeparation != null && employee.DateOfSeparation <= DateTime.Today)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "Employee separated");
            return Unauthorized("Employee is separated already");
        }

        //for IsActive column in DB can be either this or Date of Separation
        var userEntity = await _context.Users
        .FirstOrDefaultAsync(u => u.UserId == user.UserId);

        if (!userEntity.IsActive)
        {
            LogAudit(request.Username, "LOGIN_FAILED", "User is inactive");
            return Unauthorized("Employee is no longer active");
        }

        //bool hasTimedIn = await _context.Attendance.AnyAsync(a =>
        //    a.EmployeeId == employee.EmployeeId &&
        //    a.LogDate.Date == DateTime.Today &&
        //    a.TimeIn != null);

        //if (!hasTimedIn)
        //{
        //    LogAudit(request.Username, "LOGIN_FAILED", "No time-in today");
        //    return Unauthorized("No time-in record found");
        //}

        LogAudit(request.Username, "LOGIN_SUCCESS", "Login successful");

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
