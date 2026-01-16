namespace SSO.Auth.Api.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }

        // IMPORTANT: This is what VAMS will use
        public string EmployeeId { get; set; }

        public string EmployeeNo { get; set; }
        public string FullName { get; set; }
        public string Division { get; set; }
    }


}
