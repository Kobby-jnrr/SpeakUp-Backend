using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Report;
using SpeakUp.API.Models.ChatModel;
using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.UserModel;
using SpeakUp.API.Services;
using System.Security.Claims;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;

    public ReportController( ApplicationDbContext context, AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    private async Task<string> GenerateReportCode(string firstName, string lastName)
    {
        var initials =
            $"{firstName?.FirstOrDefault()}{lastName?.FirstOrDefault()}"
            .ToUpper();

        var lastId = await _context.Reports
            .OrderByDescending(r => r.Id)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var next = lastId + 1;

        return $"{initials}-{next.ToString("D4")}";
    }

    // STUDENT: CREATE REPORT
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateReport(CreateReportDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var userId = int.Parse(claim.Value);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var code = await GenerateReportCode(user.FirstName, user.LastName);

        var report = new Report
        {
            Title = $"Complaint {code}",
            Description = dto.Description,
            StudentId = userId,
            Status = ReportStatus.Pending,

            ComplainantGender = dto.ComplainantGender,
            ComplainantStudentId = dto.ComplainantStudentId,
            Department = dto.Department,
            ContactNumber = dto.ContactNumber,
            Email = dto.Email,

            RespondentName = dto.RespondentName,
            RespondentPosition = dto.RespondentPosition,
            RespondentDepartment = dto.RespondentDepartment,
            RelationshipToComplainant = dto.RelationshipToComplainant,

            IncidentDate = dto.IncidentDate,
            IncidentTime = dto.IncidentTime,
            IncidentLocation = dto.IncidentLocation,

            ComplaintNature = dto.ComplaintNature != null
                ? string.Join(", ", dto.ComplaintNature)
                : "",

            Witness1Name = dto.Witness1Name,
            Witness1Contact = dto.Witness1Contact,
            Witness2Name = dto.Witness2Name,
            Witness2Contact = dto.Witness2Contact,

            PriorReportWhere = dto.PriorReportWhere,
            DesiredOutcome = dto.DesiredOutcome,
            Confidential = dto.Confidential
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Report created successfully",
            report.Id,
            ReportCode = $"REP-{report.Id.ToString().PadLeft(6, '0')}",
            report.Title,
            report.Status,
            report.CreatedAt,
            report.ComplainantStudentId,
            report.IncidentDate,
            report.IncidentLocation
        });
    }

    // STUDENT: CREATE QUICK REPORT
    [Authorize]
    [HttpPost("quick")]
    public async Task<IActionResult> CreateQuickReport(CreateQuickReportDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            return Unauthorized();

        var userId = int.Parse(claim.Value);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized();


        var code = await GenerateReportCode(user.FirstName, user.LastName);


        var report = new Report
        {
            Title = $"Quick Report {code}",

            Description = dto.Description,
            StudentId = userId,
            Status = ReportStatus.Pending,
            Type = ReportType.Quick,

            ComplainantGender = dto.IsAnonymous
                ? null
                : user.Gender,

            Department = dto.IsAnonymous
                ? null
                : user.Department,

            ContactNumber = dto.IsAnonymous
                ? null
                : user.PhoneNumber,

            Email = dto.IsAnonymous
                ? null
                : user.Email,

            Confidential = dto.IsAnonymous
        };


        _context.Reports.Add(report);

        await _context.SaveChangesAsync();


        return Ok(new
        {
            message = "Quick report submitted successfully",

            report.Id,

            ReportCode = $"REP-{report.Id.ToString().PadLeft(6, '0')}",

            report.Title,

            report.Type,

            report.Status,

            report.CreatedAt
        });
    }

    // ADMIN: GET ALL REPORTS
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllReports()
    {
        var reports = await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                ReportCode = $"REP-{r.Id.ToString().PadLeft(6, '0')}",
                r.Title,
                r.Description,
                r.Type,
                r.Status,
                r.CreatedAt,
                r.UpdatedAt,

                r.ComplainantGender,
                r.ComplainantStudentId,
                r.Department,
                r.ContactNumber,
                r.Email,

                r.RespondentName,
                r.RespondentPosition,
                r.RespondentDepartment,
                r.RelationshipToComplainant,

                r.IncidentDate,
                r.IncidentTime,
                r.IncidentLocation,

                ComplaintNature = (r.ComplaintNature ?? "")
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries),

                r.Witness1Name,
                r.Witness1Contact,
                r.Witness2Name,
                r.Witness2Contact,

                r.PriorReportWhere,
                r.DesiredOutcome,
                r.Confidential,

                Student = r.Confidential
                ? null
                : r.Student == null ? null : new
                {
                    r.Student.Id,
                    r.Student.FirstName,
                    r.Student.LastName,
                    r.Student.Email
                },

                AssignedAdmin = r.AssignedAdmin == null ? null : new
                {
                    r.AssignedAdmin.Id,
                    r.AssignedAdmin.FirstName,
                    r.AssignedAdmin.LastName
                }
            })
            .ToListAsync();

        return Ok(reports);
    }

    // ADMIN: CLAIM REPORT
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPost("claim/{reportId}")]
    public async Task<IActionResult> ClaimReport(int reportId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var adminId = int.Parse(claim.Value);

        var report = await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.AssignedAdmin)
            .FirstOrDefaultAsync(r => r.Id == reportId); ;

        if (report == null) return NotFound("Report not found");

        if (report.AssignedAdminId != null)
            return BadRequest("Report already assigned");

        report.AssignedAdminId = adminId;
        report.Status = ReportStatus.InProgress;

        report.LastModifiedById = adminId;
        report.LastModifiedAt = DateTime.UtcNow;

        var chat = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.ReportId == reportId);

        if (chat != null)
        {
            chat.AssignedAdminId = adminId;
            chat.Status = ConversationStatus.Open;
        }

        await _context.SaveChangesAsync();

        var studentName = report.Student != null
            ? $"{report.Student.FirstName} {report.Student.LastName}"
            : "Anonymous Student";

         var admin = await _context.Users.FindAsync(adminId);

          await _auditService.Log(
              adminId,
              "Claimed Report",
              $"{studentName}'s report was claimed by {admin!.FirstName} {admin.LastName}."
              );

        return Ok(new
        {
            message = "Report claimed successfully",
            report.Id,
            report.Status,
            report.AssignedAdminId,
            report.LastModifiedAt
        });
    }

    // ADMIN: GET MY ASSIGNED REPORTS
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpGet("assigned-to-me")]
    public async Task<IActionResult> GetAssignedToMe()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var adminId = int.Parse(claim.Value);

        var reports = await _context.Reports
            .Where(r => r.AssignedAdminId == adminId)
            .Include(r => r.Student)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => new
            {
                r.Id,
                ReportCode = $"REP-{r.Id.ToString().PadLeft(6, '0')}",
                r.Title,
                r.Description,
                r.Type,
                r.Status,
                r.CreatedAt,
                r.UpdatedAt,
                r.IncidentDate,
                r.IncidentLocation,
                r.Department,
                r.ComplainantStudentId,
                r.Confidential,

                Student = r.Student == null ? null : new
                {
                    r.Student.Id,
                    r.Student.FirstName,
                    r.Student.LastName,
                    r.Student.Email
                },

                AssignedAdmin = r.AssignedAdmin == null ? null : new
                {
                    r.AssignedAdmin.Id,
                    r.AssignedAdmin.FirstName,
                    r.AssignedAdmin.LastName
                }
            })
            .ToListAsync();

        return Ok(reports);
    }

    // ADMIN: UPDATE STATUS
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPut("status/{reportId}")]
    public async Task<IActionResult> UpdateStatus(int reportId, UpdateStatusDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var userId = int.Parse(claim.Value);

        var report = await _context.Reports
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return NotFound("Report not found");

        var isSuperAdmin = User.IsInRole("SuperAdmin");

        // 🚨 HARD LOCK: Closed reports
        if (report.Status == ReportStatus.Closed && !isSuperAdmin)
            return StatusCode(StatusCodes.Status403Forbidden,
                "Closed reports can only be modified by SuperAdmin");

        // 🚨 Only assigned admin can update (except SuperAdmin)
        if (!isSuperAdmin && report.AssignedAdminId != userId)
            return StatusCode(StatusCodes.Status403Forbidden,
    "You can only update assigned reports");

        // 🟡 If moving TO Resolved, stamp time
        if (dto.Status == ReportStatus.Resolved && report.Status != ReportStatus.Resolved)
        {
            report.ResolvedAt = DateTime.UtcNow;
        }

        // 🔴 If moving TO Closed, stamp close time
        if (dto.Status == ReportStatus.Closed)
        {
            report.ClosedAt = DateTime.UtcNow;
        }

        var oldStatus = report.Status;
        report.Status = dto.Status;
        report.UpdatedAt = DateTime.UtcNow;

        report.LastModifiedById = userId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var studentName = report.Student != null
            ? $"{report.Student.FirstName} {report.Student.LastName}"
            : "Anonymous Student";

        await _auditService.Log(
            userId,
            "Changed Report Status",
            $"{studentName}'s report status changed from {oldStatus} to {dto.Status}."
        );

        return Ok(new
        {
            message = "Status updated",
            report.Id,
            report.Status
        });
    }

    // SUPER ADMIN: REASSIGN REPORT
    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("reassign/{reportId}")]
    public async Task<IActionResult> ReassignReport(int reportId, int newAdminId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var superAdminId = int.Parse(claim.Value);

        var report = await _context.Reports
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
            return NotFound("Report not found");

        if (report.AssignedAdminId == newAdminId)
            return BadRequest("Already assigned to this admin");

        var newAdmin = await _context.Users.FindAsync(newAdminId);
        if (newAdmin == null || newAdmin.Role != UserRole.JuniorAdmin)
            return BadRequest("Invalid admin selected");

        report.PreviousAdminId = report.AssignedAdminId;
        report.AssignedAdminId = newAdminId;

        report.ReassignedAt = DateTime.UtcNow;
        report.LastModifiedById = superAdminId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var studentName = report.Student != null
            ? $"{report.Student.FirstName} {report.Student.LastName}"
            : "Anonymous Student";

        await _auditService.Log(
            superAdminId,
            "Reassigned Report",
            $"{studentName}'s report was reassigned to {newAdmin.FirstName} {newAdmin.LastName}."
        );

        return Ok(new
        {
            message = "Report reassigned successfully",
            from = report.PreviousAdminId,
            to = report.AssignedAdminId,
            time = report.ReassignedAt
        });
    }

    // STUDENT: GET MY REPORTS
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReports()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var userId = int.Parse(claim.Value);

        var reports = await _context.Reports
            .Where(r => r.StudentId == userId)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,

                ReportCode = $"REP-{r.Id.ToString().PadLeft(6, '0')}",

                r.Title,
                r.Description,
                r.Status,
                r.CreatedAt,
                r.Type,

                AssignedAdmin = r.AssignedAdmin == null
                    ? null
                    : new
                    {
                        r.AssignedAdmin.Id,
                        r.AssignedAdmin.FirstName,
                        r.AssignedAdmin.LastName
                    }
            })
            .ToListAsync();

        return Ok(reports);
    }
}