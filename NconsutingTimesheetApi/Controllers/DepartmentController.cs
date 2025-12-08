using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NconsultingTimesheetApi.Data;
using NconsultingTimesheetApi.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NconsultingTimesheetApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class DepartmentController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public DepartmentController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet("GetAllDepartment")]
		public async Task<IActionResult> GetDepartmentTimings()
		{
			var result = await _context.DepartmentNameRequests
				.FromSqlRaw("EXEC GetAllDepartment")
				.ToListAsync();

			return Ok(result);
		}


		[HttpPost("InsertOrUpdateDepartment")]
		public async Task<IActionResult> SaveOrUpdateDepartmentTiming([FromBody] DepartmentNameRequest request)
		{
			string userEmail = HttpContext.User.Identity.Name;

			var result = await _context.DepartmentNameRequests
				.FromSqlRaw(
					"EXEC InsertOrUpdateDepartment @p0, @p1, @p2, @p3, @p4, @p5",
					request.DeptId,
					request.DepartmentName,
					request.StartTime,
					request.EndTime,
					request.Location,
					userEmail
				)
				.ToListAsync();

			// If result contains only Status (Insert case)
			if (result.Count == 1 && result.First().DepartmentName == null)
			{
				return Ok(new
				{
					message = "Department added successfully"
				});
			}

			// If update
			return Ok(new
			{
				message = "Department updated successfully",
				data = result.FirstOrDefault()
			});
		}




	}
}
