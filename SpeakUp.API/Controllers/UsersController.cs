using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Users
            .Where(u => u.Role == UserRole.Student)
            .OrderByDescending(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.Gender,
                u.Department,
                u.Role
            })
            .ToListAsync();

        return Ok(students);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("admins")]
    public async Task<IActionResult> GetAdmins()
    {
        var admins = await _context.Users
            .Where(u => u.Role == UserRole.JuniorAdmin || u.Role == UserRole.SuperAdmin)
            .OrderByDescending(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.Role
            })
            .ToListAsync();

        return Ok(admins);
    }
}