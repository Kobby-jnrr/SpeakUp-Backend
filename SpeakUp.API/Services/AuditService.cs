using SpeakUp.API.Data;
using SpeakUp.API.Models.AuditModel;

namespace SpeakUp.API.Services;

public class AuditService
{
    private readonly ApplicationDbContext _context;


    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }



    public async Task Log(
        int? userId,
        string action,
        string description)
    {

        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };


        _context.AuditLogs.Add(log);

        await _context.SaveChangesAsync();
    }
}