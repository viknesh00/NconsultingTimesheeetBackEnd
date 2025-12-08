namespace NconsultingTimesheetApi.Models
{
	public class TimesheetRequest
	{
		public string MonthYear { get; set; }
		public string Status { get; set; }
		public int WorkingDays { get; set; }
		public decimal TotalHours { get; set; }
		public bool IsApprovedHR { get; set; }
		public bool IsApprovedTimesheetManager { get; set; }
		public bool IsLocked { get; set; }

		public List<DailyTimesheet> Timesheet { get; set; } = new List<DailyTimesheet>();
		public string? PdfBase64 { get; set; }        // optional PDF as base64
		public string? fileName { get; set; }     // optional filename

	}

	public class DailyTimesheet
	{
		public DateTime Date { get; set; }
		public decimal? WorkingHours { get; set; }
		public string? LeaveType { get; set; }
		public string? Comment { get; set; }
		public string? PayCode { get; set; }
	}
}
