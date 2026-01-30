using System;

namespace SSO.Auth.Api.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;  // "Login Attempt" or "Login Success"
        public string Reason { get; set; } = string.Empty;  // e.g., "Invalid password", "Account inactive"
        public string ClientApp { get; set; } = "UNKNOWN";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
