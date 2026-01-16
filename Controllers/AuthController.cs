using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Auth.Api.Data;
using SSO.Auth.Api.DTOs;
using SSO.Auth.Api.Models;
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
    .FirstOrDefaultAsync(u => EF.Functions.Collate(u.Username, "Latin1_General_CS_AS") == request.Username);

            if (user == null || user.PasswordHash != request.Password)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Invalid credentials");
                return Unauthorized("Invalid credentials");
            }

            var personnel = await _context.PersonnelDivisionDetails
    .FirstOrDefaultAsync(p => p.employee_id == user.EmployeeId.ToString());

            if (personnel == null)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Personnel record missing");
                return Unauthorized("Employee record not found");
            }

            if (personnel.separation_date != null &&
    personnel.separation_date <= DateTime.Today)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "Employee separated");
                return Unauthorized("Employee is no longer active");
            }


            //for IsActive column in DB can be either this or Date of Separation
            var userEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (!userEntity.IsActive)
            {
                LogAudit(request.Username, "LOGIN_FAILED", "User is inactive");
                return Unauthorized("Employee is no longer active");
            }


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

            LogAudit(request.Username, "LOGIN_SUCCESS", "Login successful");



            return Ok(new LoginResponse
            {
                Token = "TEST-TOKEN", // temporary
                EmployeeId = personnel.employee_id, // ✅ THIS IS THE 
                EmployeeNo = personnel.employee_id.ToString(),
                FullName = $"{personnel.given_name} {personnel.surname}",
                Division = personnel.division_name
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
        // NOTE:
        // Claims will be populated by IdentityServer later.
        // This endpoint is already correct and does not need changes now.

        return Ok(User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        }));
    }

}
