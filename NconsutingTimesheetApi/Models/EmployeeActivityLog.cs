namespace NconsultingTimesheetApi.Models
{
	public class EmployeeActivityLog
	{
		public int? LogId { get; set; }
		public string UserName { get; set; }
		public string FullName { get; set; }
		public string Role { get; set; }
		public string ActionType { get; set; }
		public string MonthYear { get; set; }
		public DateTime PerformedAt { get; set; }
	}
}
