namespace NconsultingTimesheetApi.Models
{
	public class DepartmentNameRequest
	{
		public int? DeptId { get; set; }
		public string DepartmentName { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string Location { get; set; }
	}
}
