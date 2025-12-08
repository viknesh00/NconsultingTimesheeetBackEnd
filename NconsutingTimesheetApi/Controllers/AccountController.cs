using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NconsultingTimesheetApi.Data;
using NconsultingTimesheetApi.Models;
using System.Threading.Tasks;

namespace NconsultingTimesheetApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public AccountController(ApplicationDbContext context)
		{
			_context = context;
		}

		// POST: api/User/ChangePassword
		[HttpPost("ChangePassword")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
		{
			if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
				return BadRequest(new { Message = "All fields are required." });

			// Get logged-in username (email/employeeId)
			string userName = HttpContext.User.Identity.Name;

			// Fetch user login record
			var userLogin = await _context.UserLogins
				.FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive);

			if (userLogin == null)
				return NotFound(new { Message = "User not found." });

			// Verify current password
			bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, userLogin.PasswordHash);
			if (!isCurrentPasswordValid)
				return BadRequest(new { Message = "Current password is incorrect." });

			// Check if new password is same as old password
			if (BCrypt.Net.BCrypt.Verify(request.NewPassword, userLogin.PasswordHash))
				return BadRequest(new { Message = "New password cannot be same as current password." });

			// Hash new password
			string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

			// Update password in UserLogins table
			userLogin.PasswordHash = newHashedPassword;
			userLogin.IsDefaultPasswordChanged = true;

			_context.UserLogins.Update(userLogin);
			await _context.SaveChangesAsync();

			return Ok(new { Message = "Password changed successfully." });
		}

		[HttpGet("GetManagerLists")]
		public async Task<IActionResult> GetNonEmployeeUsers()
		{
			var result = await _context.DropDownItems
				.FromSqlRaw("EXEC GetNonEmployeeUsers")
				.ToListAsync();

			return Ok(result);
		}

		[HttpGet("GetTaskManager")]
		public async Task<IActionResult> GetManagerLists()
		{
			// HR Manager emails
			var hrManagers = await _context.DropDownItems
				.FromSqlRaw("EXEC TaskManager @p0", "HR Manager")
				.ToListAsync();

			// Timesheet Approver emails
			var timesheetApprovers = await _context.DropDownItems
				.FromSqlRaw("EXEC TaskManager @p0", "Timesheet Approver")
				.ToListAsync();

			// Return separately
			return Ok(new
			{
				HRManagers = hrManagers,
				TimesheetApprovers = timesheetApprovers
			});
		}



	}
}
