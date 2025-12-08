using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NconsultingTimesheetApi.Data;       // your DBContext namespace
using NconsultingTimesheetApi.Models;    // your UserDto namespace

namespace NconsultingTimesheetApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;

		public UserController(ApplicationDbContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		// GET: api/User/All
		[HttpGet("All")]
		public async Task<IActionResult> GetAllUsers()
		{
			var users = await _context.UserLists
				.FromSqlRaw("EXEC Get_All_Users")
				.ToListAsync();

			return Ok(users);
		}

		[HttpGet("GetUser/{UserID}")]
		public IActionResult GetUserByEmail(string UserID)
		{
			var result = _context.Users
				.FromSqlRaw("EXEC GetUser @p0", UserID)
				.ToList();

			return Ok(result);
		}


		[HttpPost("Add")]
		public async Task<IActionResult> AddUser([FromBody] User user)
		{
			var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
			if (emailExists)
			{
				return Conflict(new { Message = "Email already exists." });
			}

			// Generate default password and hash it
			string defaultPassword = "Welcome@123"; // You can also generate dynamically if needed
			string passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

			await _context.Database.ExecuteSqlRawAsync(
				@"EXEC AddUser 
            @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14",
				user.FirstName, user.LastName,user.Email,
				user.EmployeeType, user.Department, user.Designation, user.DOJ, user.WorkLocation,
				user.ReportingManager, user.AccessRole, user.EmploymentStatus, user.EmployeeId,
				passwordHash,user.Country,user.City // pass the hashed password as last parameter
			);

			string subject = "Welcome to the Company!";
			string body = $@"
			<html>
			<body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
			<div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
            <h2 style='color: #2E86C1;'>{subject}</h2>
            <p>Dear {user.FirstName} {user.LastName},</p>
            <p>Your account has been created successfully. Please find your login details below:</p>
            <ul>
                <li><strong>Username:</strong> {user.Email}</li>
                <li><strong>Password:</strong> {defaultPassword}</li>
            </ul>
            <p>Please change your password after your first login.</p>
            <p style='margin-top: 20px;'>If you have any questions, please contact HR.</p>
            <p style='color:gray;'>This is a system-generated email</p>
            <p>Regards,<br/>Timesheet Team<br/>
				<img src='http://www.natobotics.com/img/Natobotics.png' alt='Company Logo' style='width:50px; height:auto; display:block;' />
				<br/>
				e: timesheet@n-cons.co.uk<br/>
				a: N Consulting Ltd, 14 - 16 Dowgate Hill, London, EC4R 2SU<br/>
				w: www.n-cons.co.uk
			</p>
			</div>
			</body>
			</html>";

			// Send the email
			await _emailService.SendEmail(
				to: user.Email,
				subject: subject,
				body: body,
				cc: null // or add HR/admin email if needed
			);

			return Ok(new { Message = "User Created Successfully", DefaultPassword = defaultPassword });
		}

		[HttpPost("Edit")]
		public async Task<IActionResult> EditUser([FromBody] User user)
		{
			await _context.Database
				.ExecuteSqlRawAsync("EXEC UpdateUser @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13",
					user.Email, user.FirstName, user.LastName, 
					 user.EmployeeType, user.Department, user.Designation,
					user.DOJ, user.WorkLocation, user.ReportingManager, user.AccessRole,
					user.EmploymentStatus, user.EmployeeId, user.Country, user.City
				);
			return Ok(new { Message = "User Updated Successfully" });
		}

		[HttpPost("UpdateUserStaus")]
		public async Task<IActionResult> UpdateIsActive([FromBody] StatusUpdateRequest request)
		{
			if (string.IsNullOrEmpty(request.UserName))
				return BadRequest(new { Message = "UserName is required." });

			await _context.Database.ExecuteSqlRawAsync(
				"EXEC UpdateUserStaus @p0, @p1",
				request.UserName,
				request.IsActive
			);

			return Ok(new { Message = "Status updated successfully." });
		}


		[HttpGet("GetUserList")]
		public async Task<IActionResult> GetUserList()
		{
			// Get logged-in user's email
			string userName = HttpContext.User.Identity.Name;

			// Call the stored procedure and map to DTO
			var result = await _context.UserBasedRoles
				.FromSqlRaw("EXEC GetUserList @p0", userName)
				.ToListAsync();

			return Ok(result);
		}





		[HttpGet("CheckEmail")]
		public async Task<IActionResult> CheckEmail(string email)
		{
			var result = await _context.EmailCheckResponses
				.FromSqlRaw("EXEC CheckEmailExists @p0", email)
				.ToListAsync();

			// Return true/false as boolean
			bool exists = result.Count > 0 && result[0].EmailExists == 1;

			return Ok(new { EmailExists = exists });
		}




	}
}
