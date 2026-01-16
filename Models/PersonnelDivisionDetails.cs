namespace SSO.Auth.Api.Models
{
    /// Read-only model mapped to vw_PersonnelDivisionDetails
    public class PersonnelDivisionView
    {
        public int hris_id { get; set; }
        public string employee_id { get; set; }
        public string surname { get; set; } = string.Empty;
        public string given_name { get; set; } = string.Empty;
        public DateTime? separation_date { get; set; }
        public string division_name { get; set; } = string.Empty;
    }
}
