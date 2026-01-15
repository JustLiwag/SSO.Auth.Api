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

   

    public AuthController(AppDbContext context)
    {
        _context = context;
        
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
    .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || user.PasswordHash != request.Password)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Invalid credentials");
                return Unauthorized("Invalid credentials");
            }

            var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);

            if (employee == null)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Employee record missing");
                return Unauthorized("Employee record not found");
            }

            if (employee.DateOfSeparation != null)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Employee separated");
                return Unauthorized("Employee is no longer active");
            }


            bool hasTimedIn = await _context.Attendance.AnyAsync(a =>
                a.EmployeeId == employee.EmployeeId &&
                a.LogDate.Date == DateTime.Today &&
                a.TimeIn != null);

            //if (!hasTimedIn)
            //{
            //    LogAudit(request.Username, "LOGIN_FAILED", "No time-in today");
            //    return Unauthorized("No time-in record found");
            //}

            LogAudit(request.Username, "LOGIN_SUCCESS", "Login successful");

            

            return Ok(new LoginResponse
            {
                Token = "TEST-TOKEN",
                EmployeeNo = employee.EmployeeNo,
                FullName = employee.FullName,
                Division = employee.Division
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
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
