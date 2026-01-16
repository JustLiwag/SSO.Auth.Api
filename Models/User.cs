namespace SSO.Auth.Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; }
        public int EmployeeId { get; set; }

        public DateTime? LockoutUntil { get; set; }
    }

}
