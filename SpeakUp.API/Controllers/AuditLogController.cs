using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuditController(ApplicationDbContext context)
    {
        _context = context;
    }


    // SUPER ADMIN: GET ALL AUDIT LOGS
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.Action,
                a.Description,
                a.CreatedAt,

                UserId = a.UserId,

                UserName = a.User != null
                    ? $"{a.User.FirstName} {a.User.LastName}"
                    : "System"
            })
            .ToListAsync();


        return Ok(logs);
    }



    // SUPER ADMIN: GET RECENT LOGS
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent()
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new
            {
                a.Id,
                a.Action,
                a.Description,
                a.CreatedAt,

                UserName = a.User != null
                    ? $"{a.User.FirstName} {a.User.LastName}"
                    : "System"
            })
            .ToListAsync();


        return Ok(logs);
    }
}