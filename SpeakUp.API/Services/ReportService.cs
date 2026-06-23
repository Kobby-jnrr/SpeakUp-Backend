using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.Models.ReportModel;

namespace SpeakUp.API.Services;

public class ReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AutoCloseResolvedReportsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-14);

        var reportsToClose = await _context.Reports
            .Where(r =>
                r.Status == ReportStatus.Resolved &&
                r.ResolvedAt != null &&
                r.ResolvedAt <= cutoff)
            .ToListAsync();

        foreach (var report in reportsToClose)
        {
            report.Status = ReportStatus.Closed;
            report.ClosedAt = DateTime.UtcNow;
        }

        if (reportsToClose.Any())
            await _context.SaveChangesAsync();
    }

    public async Task<Report> CreateReportAsync(Report report)
    {
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<List<Report>> GetAllReportsAsync()
    {
        await AutoCloseResolvedReportsAsync();

        return await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(int id)
    {
        await AutoCloseResolvedReportsAsync();

        return await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.AssignedAdmin)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ClaimReportAsync(Report report, int adminId)
    {
        if (report.Status == ReportStatus.Closed)
            return false;

        if (report.AssignedAdminId != null)
            return false;

        report.AssignedAdminId = adminId;
        report.Status = ReportStatus.InProgress;

        report.LastModifiedById = adminId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(
        Report report,
        ReportStatus newStatus,
        int userId,
        bool isSuperAdmin = false)
    {
        if (report.Status == ReportStatus.Closed && !isSuperAdmin)
            return false;

        if (!isSuperAdmin && report.AssignedAdminId != userId)
            return false;

        if (newStatus == ReportStatus.Resolved && report.Status != ReportStatus.Resolved)
        {
            report.ResolvedAt = DateTime.UtcNow;
        }

        if (newStatus == ReportStatus.Closed)
        {
            report.ClosedAt = DateTime.UtcNow;
        }

        report.Status = newStatus;
        report.UpdatedAt = DateTime.UtcNow;

        report.LastModifiedById = userId;
        report.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Report>> GetMyReportsAsync(int userId)
    {
        await AutoCloseResolvedReportsAsync();

        return await _context.Reports
            .Where(r => r.StudentId == userId)
            .Include(r => r.AssignedAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}