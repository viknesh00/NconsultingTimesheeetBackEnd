namespace NconsultingTimesheetApi.Models
{
	public class TaskDetailsEmployee
	{
		public string Project { get; set; }
		public string Client { get; set; }
		public string Task { get; set; }
		public string TimesheetApprover { get; set; }
		public string HRApprover { get; set; }
		public bool EnableWeekend { get; set; }
		public bool AllowOvertime { get; set; }
	}
}
