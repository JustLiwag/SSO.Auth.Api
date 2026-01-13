namespace SSO.Auth.Api.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime LogDate { get; set; }
        public DateTime? TimeIn { get; set; }
    }

}
