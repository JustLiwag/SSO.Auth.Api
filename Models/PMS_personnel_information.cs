namespace SSO.Auth.Api.Models
{
    public class PMS_personnel_information
    {
        public int hris_id { get; set; }           // Primary Key
        public string employee_id { get; set; } = string.Empty;
        public string surname { get; set; } = string.Empty;
        public string given_name { get; set; } = string.Empty;
        public DateTime? separation_date { get; set; }
        public string division_name { get; set; } = string.Empty;
    }

}
