using System.ComponentModel.DataAnnotations;

namespace NconsutingTimesheetApi.Models
{
	public class Project
	{
		public int ProjectId { get; set; }
		public string ProjectName { get; set; }
		public string PONumber { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime? UpdatedAt { get; set; }
	}
}
