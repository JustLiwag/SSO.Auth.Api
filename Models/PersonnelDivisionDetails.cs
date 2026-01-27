namespace SSO.Auth.Api.Models
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]   // Required for EF Core
    public class PersonnelDivisionView
    {
        public int hris_id { get; set; }
        public string employee_id { get; set; } = string.Empty;
        public string surname { get; set; } = string.Empty;
        public string given_name { get; set; } = string.Empty;
        public string division_name { get; set; } = string.Empty;
        public DateTime? separation_date { get; set; }
    }

}
