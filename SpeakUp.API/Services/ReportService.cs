using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.Models.ReportModel;
using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Services;

public class ReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Report> CreateReportAsync(Report report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<List<Report>> GetAllReportsAsync()
    {
        return await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(int id)
    {
        return await _context.Reports.FindAsync(id);
    }

    public async Task<bool> ClaimReportAsync(Report report, int adminId)
    {
        if (report.AssignedAdminId != null)
            return false;

        report.AssignedAdminId = adminId;
        report.Status = ReportStatus.InProgress;
        report.LastModifiedById = adminId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Report report, ReportStatus status, int userId)
    {
        report.Status = status;
        report.UpdatedAt = DateTime.UtcNow;
        report.LastModifiedById = userId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Report>> GetMyReportsAsync(int userId)
    {
        return await _context.Reports
            .Where(r => r.StudentId == userId)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}