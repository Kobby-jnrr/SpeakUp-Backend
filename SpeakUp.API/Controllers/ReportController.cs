using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Report;
using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.UserModel;
using System.Security.Claims;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    // STUDENT: CREATE REPORT
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateReport(CreateReportDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var userId = int.Parse(claim.Value);

        var report = new Report
        {
            Title = dto.Title,
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
            report.Title,
            report.Status,
            report.CreatedAt,
            report.ComplainantStudentId,
            report.IncidentDate,
            report.IncidentLocation
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
                r.Title,
                r.Description,
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

    // ADMIN: CLAIM REPORT
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPost("claim/{reportId}")]
    public async Task<IActionResult> ClaimReport(int reportId)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var adminId = int.Parse(claim.Value);

        var report = await _context.Reports.FindAsync(reportId);
        if (report == null) return NotFound("Report not found");

        if (report.AssignedAdminId != null)
            return BadRequest("Report already assigned");

        report.AssignedAdminId = adminId;
        report.Status = ReportStatus.InProgress;

        report.LastModifiedById = adminId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Report claimed successfully",
            report.Id,
            report.Status,
            report.AssignedAdminId,
            report.LastModifiedAt
        });
    }

    // ADMIN: UPDATE STATUS
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPut("status/{reportId}")]
    public async Task<IActionResult> UpdateStatus(int reportId, UpdateStatusDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) return Unauthorized();

        var userId = int.Parse(claim.Value);

        var report = await _context.Reports.FindAsync(reportId);
        if (report == null) return NotFound("Report not found");

        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (!isSuperAdmin && report.AssignedAdminId != userId)
            return Forbid("You can only update assigned reports");

        report.Status = dto.Status;
        report.UpdatedAt = DateTime.UtcNow;

        report.LastModifiedById = userId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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

        var report = await _context.Reports.FindAsync(reportId);
        if (report == null) return NotFound("Report not found");

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
                r.Title,
                r.Description,
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

                ComplaintNature = string.IsNullOrWhiteSpace(r.ComplaintNature)
                ? new string[0]
                : r.ComplaintNature.Split(", ", StringSplitOptions.RemoveEmptyEntries),

                r.Witness1Name,
                r.Witness1Contact,
                r.Witness2Name,
                r.Witness2Contact,

                r.PriorReportWhere,
                r.DesiredOutcome,
                r.Confidential,

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
}