using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Content;
using SpeakUp.API.Models.ContentModel;
using System.Security.Claims;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomePageContentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HomePageContentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ADMIN: CREATE CONTENT
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateContentDto dto)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            return Unauthorized();

        var userId = int.Parse(claim.Value);

        var content = new HomePageContent
        {
            Type = dto.Type,
            Title = dto.Title,
            Content = dto.Content,
            ImageUrl = dto.ImageUrl,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            CreatedById = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.HomePageContents.Add(content);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Content created successfully",
            content.Id,
            content.Type,
            content.Title
        });
    }

    // ADMIN: DELETE CONTENT
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var content = await _context.HomePageContents.FindAsync(id);

        if (content == null)
            return NotFound("Content not found");

        _context.HomePageContents.Remove(content);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Content deleted successfully",
            content.Id
        });
    }

    // ADMIN: GET ALL CONTENT
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var content = await _context.HomePageContents
            .Include(c => c.CreatedBy)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new HomePageContentDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                Content = c.Content,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                StartAt = c.StartAt,
                EndAt = c.EndAt,
                CreatedBy = c.CreatedBy.FirstName + " " + c.CreatedBy.LastName
            })
            .ToListAsync();

        return Ok(content);
    }

    // ADMIN: TOGGLE ACTIVE/INACTIVE
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPut("toggle/{id}")]
    public async Task<IActionResult> Toggle(int id)
    {
        var content = await _context.HomePageContents.FindAsync(id);

        if (content == null)
            return NotFound("Content not found");

        content.IsActive = !content.IsActive;
        content.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Content status updated",
            content.Id,
            content.IsActive
        });
    }

    // STUDENT: GET ACTIVE HOMEPAGE CONTENT
    [AllowAnonymous]
    [HttpGet("home")]
    public async Task<IActionResult> GetHome()
    {
        var now = DateTime.UtcNow;

        var content = await _context.HomePageContents
            .Where(c =>
                c.IsActive &&
                (c.StartAt == null || c.StartAt <= now) &&
                (c.EndAt == null || c.EndAt >= now))
            .ToListAsync();

        var hero = content
            .Where(c => c.Type == ContentType.Hero)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new HomePageContentDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                Content = c.Content,
                ImageUrl = c.ImageUrl,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        var bulletin = content
            .Where(c => c.Type == ContentType.Bulletin)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new HomePageContentDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                Content = c.Content,
                ImageUrl = c.ImageUrl,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        var safetyTips = content
            .Where(c => c.Type == ContentType.SafetyTip)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var randomSafetyTip = safetyTips
            .OrderBy(_ => Guid.NewGuid())
            .Select(c => new HomePageContentDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = c.Title,
                Content = c.Content,
                ImageUrl = c.ImageUrl,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefault();

        return Ok(new
        {
            hero,
            bulletin,
            safetyTip = randomSafetyTip
        });
    }
}