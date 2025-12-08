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
	public class TaskController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public TaskController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet("GetTasks")]
		public async Task<IActionResult> GetTasks()
		{
			string userName = HttpContext.User.Identity.Name;

			var tasks = await _context.TaskRequests
				.FromSqlRaw("EXEC GetTasksByUser @p0", userName)
				.ToListAsync();

			return Ok(tasks);
		}


		[HttpPost("SaveTask")]
		public async Task<IActionResult> SaveTask([FromBody] TaskRequest model)
		{

			var result = await _context.TaskRequests
				.FromSqlRaw(
					"EXEC InsertOrUpdateTask @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10",
					model.TaskId ?? (object)DBNull.Value,
					model.UserName,
					model.EmployeeName,
					model.EmployeeId,
					model.Project,
					model.Client,
					model.Task,
					model.TimesheetApprover,
					model.HRApprover,
					model.EnableWeekend,
					model.AllowOvertime
				)
				.ToListAsync();

			return Ok(new { message = "Task saved successfully!", data = result.FirstOrDefault() });
		}


		[HttpGet("TaskUserList")]
		public async Task<IActionResult> GetUserTaskList()
		{
			// Get logged-in user's email
			string userName = HttpContext.User.Identity.Name;

			// Call the stored procedure and map to DTO
			var result = await _context.UserBasedRoles
				.FromSqlRaw("EXEC GetUserforTaskList @p0", userName)
				.ToListAsync();

			return Ok(result);
		}

	}
}
