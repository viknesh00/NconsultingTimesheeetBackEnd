namespace NconsultingTimesheetApi.Models
{
	public class EmployeeTimesheet
	{
		public string? Username { get; set; }
		public DateTime? Date { get; set; }
		public string? PayCode { get; set; }
		public decimal? WorkingHours { get; set; }
		public string? LeaveType { get; set; }
		public string? Comment { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
