using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NconsultingTimesheetApi.Models;
using NconsultingTimesheetApi.Data;
using NconsultingTimesheetApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net;
namespace NconsultingTimesheetApi.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class TimesheetController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailService _emailService;

		public TimesheetController(ApplicationDbContext context, IEmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		[HttpPost("InsertOrUpdateTimesheet")]
		public async Task<IActionResult> SaveOrUpdateTimesheet([FromBody] TimesheetRequest request)
		{
			string userName = HttpContext.User.Identity.Name;
			string role = User.FindFirst(ClaimTypes.Role)?.Value;
			string fullName = User.FindFirst("FullName")?.Value;

			// Log Daily Activity
			await _context.Database.ExecuteSqlRawAsync(
				"EXEC InsertActivityLog @p0, @p1, @p2, @p3, @p4",
				userName,
				fullName,
				role,
				request.Status,
				request.MonthYear
			);

			var dailyResults = new List<EmployeeTimesheet>();

			foreach (var day in request.Timesheet)
			{
				var result = await _context.EmployeeTimesheet
					.FromSqlRaw(
						"EXEC InsertOrUpdateTimesheet @p0, @p1, @p2, @p3, @p4, @p5",
						userName,
						day.Date,
						day.PayCode,
						day.WorkingHours,
						string.IsNullOrEmpty(day.LeaveType) ? DBNull.Value : day.LeaveType,
						string.IsNullOrEmpty(day.Comment) ? DBNull.Value : day.Comment
					)
					.ToListAsync();

				dailyResults.AddRange(result);
			}

			var summaryResult = await _context.EmployeeTimesheetSummary
				.FromSqlRaw(
					"EXEC InsertOrUpdateTimesheetSummary @p0, @p1, @p2, @p3, @p4, @p5, @p6",
					userName,
					request.MonthYear,
					request.Status,
					request.WorkingDays,
					request.TotalHours,
					request.IsApprovedHR,
					request.IsApprovedTimesheetManager,
					request.IsLocked
				)
				.ToListAsync();

			if (request.Status.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
			{
				byte[] pdfBytes = Convert.FromBase64String(request.PdfBase64);


				// Prepare attachment
				var attachment = new EmailAttachment
				{
					FileName = request.fileName,
					ContentType = "application/pdf",
					Content = pdfBytes
				};
				// Get approver emails from Tasks table
				var task = await _context.TaskRequests
					.Where(t => t.UserName == userName)
					.Select(t => new
					{
						TimesheetApprover = t.TimesheetApprover,
						HRApprover = t.HRApprover
					})
					.FirstOrDefaultAsync();

				// Get user email from Users table
				var userEmail = await _context.Users
					.Where(u => u.Email == userName)
					.Select(u => u.Email)
					.FirstOrDefaultAsync();

				if (task != null && !string.IsNullOrEmpty(userEmail))
				{
					try
					{
						// ===== Approver Email =====
						if (!string.IsNullOrEmpty(task.TimesheetApprover))
						{
							var approverSubject = $"Timesheet Submitted by {userName} for {request.MonthYear}";
							var approverBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
							 <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                            <p>Dear Approver,</p>
                            <p>The timesheet for <strong>{request.MonthYear}</strong> has been submitted by <strong>{userName}</strong> and requires your approval.</p>
                            <p><strong>Employee:</strong> {userName}<br/>
                               <strong>Month:</strong> {request.MonthYear}</p>
							<p>Please log in to the portal to review and approve the timesheet.</p>
                            <p style='color:gray;'>This is a system-generated email. Please do not reply.</p>
                            <br/>
                            <p>Regards,<br/>Timesheet System<br/>
							<img src='http://www.natobotics.com/img/Natobotics.png' alt='Company Logo' style='width:50px; height:auto; display:block;' />
							<br/>
							e: timesheet@n-cons.co.uk<br/>
							a: N Consulting Ltd, 14 - 16 Dowgate Hill, London, EC4R 2SU<br/>
							w: www.n-cons.co.uk
							</p>
							</div>
                        </body>
                        </html>";

							await _emailService.SendEmail(
								to: task.TimesheetApprover,
								subject: approverSubject,
								body: approverBody,
								cc: task.HRApprover,
								attachments: new[] { attachment }
							);
						}

						// ===== Employee Acknowledgment Email =====
						var ackSubject = $"Timesheet Submitted Successfully for {request.MonthYear}";
						var ackBody = $@"
                    <html>
                     <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
            <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <p>Dear {userName},</p>
                        <p>Your timesheet for <strong>{request.MonthYear}</strong> has been successfully submitted and sent for approval.</p>
                        <p style='color:gray;'>This is a system-generated email</p>
                        <br/>
                        <p>Regards,<br/>Timesheet System<br/>
						<img src='http://www.natobotics.com/img/Natobotics.png' alt='Company Logo' style='width:50px; height:auto; display:block;' />
						<br/>						
						e: timesheet@n-cons.co.uk<br/>
						a: N Consulting Ltd, 14 - 16 Dowgate Hill, London, EC4R 2SU<br/>
						w: www.n-cons.co.uk
						</p>
					</div>
                    </body>
                    </html>";

						//await _emailService.SendEmail(
						//	to: userEmail,
						//	subject: ackSubject,
						//	body: ackBody,
						//	attachments: new[] { attachment }
						//);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error sending emails: {ex.Message}");
					}
				}
			}

			return Ok(new
			{
				DailyRows = dailyResults,
				Summary = summaryResult.FirstOrDefault()
			});
		}

		[HttpPost("GetTimesheetByMonth")]
		public async Task<IActionResult> GetTimesheetByMonth([FromBody] TimesheetGetRequest request, [FromQuery] string username = null)
		{
			string userName = username ?? HttpContext.User.Identity.Name;


			var dailyResults = await _context.EmployeeTimesheet
				.FromSqlRaw(
					"EXEC GetTimesheet @p0, @p1",
					userName,
					request.MonthYear
				)
				.ToListAsync();

			var summaryResult = await _context.EmployeeTimesheetSummary
				.FromSqlRaw(
					"EXEC GetTimesheetSummary @p0, @p1",
					userName,
					request.MonthYear
				)
				.ToListAsync();

			var taskDetails = await _context.TaskDetailsEmployees
				.FromSqlRaw(
					"EXEC GetEmployeeTaskDetails @p0",
					userName
				)
				.ToListAsync();

			var holidays = await _context.HolidayResponses
					.FromSqlRaw("EXEC GetHolidaysByUser @p0", userName)
					.ToListAsync();

			return Ok(new
			{
				DailyRows = dailyResults,
				Summary = summaryResult.FirstOrDefault(),
				taskDetails = taskDetails,
				holidays = holidays
			});
		}

		[HttpGet("GetTimesheet")]
		public async Task<IActionResult> GetTimeSheetList([FromQuery] string month)
		{
			string userName = HttpContext.User.Identity.Name;

			var result = await _context.EmployeeTimesheetSummary
				.FromSqlRaw(
					"EXEC GetTimesheetSummaryForUser @p0,@p1",
					userName,
					month
				)
				.ToListAsync();

			return Ok(result);
		}
		[HttpPost("GetActivityLog")]
		public async Task<IActionResult> GetActivityLog([FromBody] ActivityLogRequest request, [FromQuery] string username = null)
		{
			string userName = username ?? HttpContext.User.Identity.Name;

			var activityLogs = await _context.EmployeeActivityLogs
				.FromSqlRaw(
					"EXEC GetActivityLog @p0, @p1",
					userName,
					(object?)request.MonthYear ?? DBNull.Value
				)
				.ToListAsync();

			return Ok(activityLogs);
		}

		[HttpPost("UpdateTimesheetStatus")]
		public async Task<IActionResult> SaveTask([FromBody] UpdateTimesheetStatus model)
		{

			string UpdatedBy = HttpContext.User.Identity.Name;
			await _context.Database.ExecuteSqlRawAsync(
				"EXEC UpdateTimesheetStatus @p0,@p1,@p2,@p3,@p4,@p5",
					model.Username,
					model.MonthYear,
					model.ActionType,
					model.ActionValue,
					UpdatedBy,
					model.Comment
			);
			string subject = "";
			string body = "";

			if (model.ActionType == "HR_APPROVAL")
			{
				subject = model.ActionValue ? "Timesheet HR Approved" : "Timesheet HR Not Approved";
			}
			else if (model.ActionType == "MANAGER_APPROVAL")
			{
				subject = model.ActionValue ? "Timesheet Manager Approved" : "Timesheet Manager Not Approved";
			}
			else if (model.ActionType == "LOCK")
			{
				subject = model.ActionValue ? "Timesheet Locked" : "Timesheet Unlocked";
			}

			if (!string.IsNullOrEmpty(subject))
			{
				// Build HTML body
				string statusText = "";
				string statusColor = "";

				switch (model.ActionType)
				{
					case "HR_APPROVAL":
					case "MANAGER_APPROVAL":
						statusText = model.ActionValue ? "Approved" : "Not Approved";
						statusColor = model.ActionValue ? "green" : "red";
						break;
					case "LOCK":
						statusText = model.ActionValue ? "Locked" : "Unlocked";
						statusColor = "#FFA500"; // Orange
						break;
				}

				string commentHtml = "";
				if (!model.ActionValue && !string.IsNullOrEmpty(model.Comment)) // Not approved
				{
					commentHtml = $"<p><strong>Comment:</strong> {model.Comment}</p>";
				}

				body = $@"
        <html>
        <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
            <div style='max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                <h2 style='color: #2E86C1;'>{subject}</h2>
                <p>Dear {model.Username},</p>
                <p>Your timesheet for <strong>{model.MonthYear}</strong> has been 
                    <span style='color: {statusColor}; font-weight: bold;'>{statusText}</span>.</p>
					{commentHtml}
                <p style='margin-top: 20px;'>If you have any questions, please contact HR or your manager.</p>
				<p style='color:gray;'>This is a system-generated email</p>
				<p>Regards,<br/>Timesheet System<br/>
				<img src='http://www.natobotics.com/img/Natobotics.png' alt='Company Logo' style='width:50px; height:auto; display:block;' />
				<br/>
				e: timesheet@n-cons.co.uk<br/>
				a: N Consulting Ltd, 14 - 16 Dowgate Hill, London, EC4R 2SU<br/>
				w: www.n-cons.co.uk
				</p>
            </div>
        </body>
        </html>";

				//await _emailService.SendEmail(
				//	to: model.Username,
				//	subject: subject,
				//	body: body,
				//	cc: UpdatedBy
				//);
			}
			return Ok(new { message = "Timesheet updated successfully" });
		}

		[HttpPost("BulkUpdateTimesheetStatus")]
		public async Task<IActionResult> BulkUpdateTimesheetStatus(List<UpdateTimesheetStatus> items)
		{
			string UpdatedBy = HttpContext.User.Identity.Name;
			foreach (var item in items)
			{
				await _context.Database.ExecuteSqlRawAsync(
					"EXEC UpdateTimesheetStatus @p0,@p1,@p2,@p3,@p4,@p5",
					item.Username,
					item.MonthYear,
					item.ActionType,
					item.ActionValue,
					UpdatedBy,
					item.Comment
				);
			}

			return Ok("Bulk updated successfully");
		}

	}
}
