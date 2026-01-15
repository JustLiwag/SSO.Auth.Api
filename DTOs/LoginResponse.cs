namespace SSO.Auth.Api.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string EmployeeNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
    }

}
