namespace SSO.Auth.Api.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeNo { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Division { get; set; } = null!;
        public DateTime? DateOfSeparation { get; set; }
    }

}
