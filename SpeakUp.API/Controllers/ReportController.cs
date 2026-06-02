using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Report;
using SpeakUp.API.Models.ReportModel;
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

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateReport(CreateReportDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var report = new Report
        {
            Title = dto.Title,
            Description = dto.Description,
            StudentId = userId,
            Status = ReportStatus.Pending
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Report created successfully",
            report.Id,
            report.Title,
            report.Status
        });
    }
}