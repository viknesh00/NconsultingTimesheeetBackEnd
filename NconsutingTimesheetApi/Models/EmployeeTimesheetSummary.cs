namespace NconsultingTimesheetApi.Models
{
	public class EmployeeTimesheetSummary
	{
		public string? Username { get; set; }
		public string? MonthYear { get; set; }
		public string? Status { get; set; }
		public int? WorkingDays { get; set; }
		public decimal? TotalHours { get; set; }
		public bool? IsApprovedHR { get; set; }
		public string? IsApprovedHRBy { get; set; }
		public DateTime? IsApprovedHRAt { get; set; }
		public bool? IsApprovedTimesheetManager { get; set; }
		public string? IsApprovedTimesheetManagerBy { get; set; }
		public DateTime? IsApprovedTimesheetManagerAt { get; set; }
		public bool? IsLocked { get; set; }
		public string? IsLockedBy { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
