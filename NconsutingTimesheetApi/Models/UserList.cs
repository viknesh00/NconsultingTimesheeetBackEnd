namespace NconsultingTimesheetApi.Models
{
	public class UserList
	{
		public int UserId { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? Email { get; set; }
		public string? EmployeeType { get; set; }
		public string? Department { get; set; }
		public string? Designation { get; set; }
		public DateTime? DOJ { get; set; }
		public string? WorkLocation { get; set; }
		public string? ReportingManager { get; set; }
		public string? AccessRole { get; set; }
		public string? EmploymentStatus { get; set; }
		public string? EmployeeId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		// 🔥 Added from UserLogins table
		public bool? IsActive { get; set; }
		public DateTime? LastLoginAt { get; set; }
	}
}
