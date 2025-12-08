namespace NconsutingTimesheetApi.Models
{
	public class ProjectRequest
	{
		public int? ProjectId { get; set; }
		public string ProjectName { get; set; }
		public string PONumber { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool IsActive { get; set; }
	}
}
