namespace NconsutingTimesheetApi.Models
{
	public class UpdateProjectStatusRequest
	{
		public int ProjectId { get; set; }
		public bool IsActive { get; set; }
	}
}
