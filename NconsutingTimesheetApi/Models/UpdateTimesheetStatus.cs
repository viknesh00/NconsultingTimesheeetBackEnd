namespace NconsultingTimesheetApi.Models
{
	public class UpdateTimesheetStatus
	{
		public string Username { get; set; }
		public string MonthYear { get; set; }
		public string ActionType { get; set; }
		public bool ActionValue { get; set; }
		public string? Comment { get; set; }
	}
}
