using System;
using System.ComponentModel.DataAnnotations;

namespace NconsultingTimesheetApi.Models
{
	public class User
	{
		[Key]
		public int UserId { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }	
		[EmailAddress]
		public string? Email { get; set; }
		public string? EmployeeType { get; set; }
		public string? Department { get; set; }
		public string? Designation { get; set; }
		public DateTime? DOJ { get; set; }
		public string? Country { get; set; }
		public string? City { get; set; }
		public string? WorkLocation { get; set; }
		public string? ReportingManager { get; set; }
		public string? AccessRole { get; set; }
		public string? EmploymentStatus { get; set; }
		public string? EmployeeId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime? UpdatedAt { get; set; }
	}
}
