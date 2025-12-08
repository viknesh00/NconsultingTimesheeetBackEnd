using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NconsultingTimesheetApi.Data;
using NconsultingTimesheetApi.Models;
using NconsutingTimesheetApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NconsultingTimesheetApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class ProjectController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public ProjectController(ApplicationDbContext context)
		{
			_context = context;
		}

		// Get all projects
		[HttpGet("GetAllProjects")]
		public async Task<IActionResult> GetAllProjects()
		{
			var projects = await _context.Projects
				.FromSqlRaw("EXEC GetAllProjects")
				.ToListAsync();

			return Ok(projects);
		}

		// Insert or update project
		[HttpPost("InsertOrUpdateProject")]
		public async Task<IActionResult> InsertOrUpdateProject([FromBody] ProjectRequest request)
		{
			await _context.Database.ExecuteSqlRawAsync(
				"EXEC InsertOrUpdateProject @p0, @p1, @p2, @p3, @p4, @p5",
				request.ProjectId,
				request.ProjectName,
				request.PONumber,
				request.StartDate,
				request.EndDate,
				request.IsActive
			);

			return Ok(new { message = request.ProjectId == null ? "Project added successfully" : "Project updated successfully" });
		}

		// Update project status (activate/deactivate)
		[HttpPost("UpdateProjectStatus")]
		public async Task<IActionResult> UpdateProjectStatus([FromBody] UpdateProjectStatusRequest request)
		{
			await _context.Database.ExecuteSqlRawAsync(
				"EXEC UpdateProjectStatus @p0, @p1",
				 request.ProjectId, request.IsActive
			);

			return Ok(new { message = request.IsActive ? "Project activated successfully" : "Project deactivated successfully" });
		}
	}
}
