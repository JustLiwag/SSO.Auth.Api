namespace SSO.Auth.Api.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string Username { get; set; } = null!; // matches your table
        public string Action { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }



}
