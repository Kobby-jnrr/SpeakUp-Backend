using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Resource;
using SpeakUp.API.Models.ResourceModel;
using SpeakUp.API.Models.UserModel;
using SpeakUp.API.Services;
using System.Security.Claims;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;
    private readonly NotificationService _notificationService;


    public ResourceController(
    ApplicationDbContext context,
    AuditService auditService,
    NotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
    }


    // PUBLIC: GET PUBLISHED
    [HttpGet]
    public async Task<ActionResult<List<ResourceDto>>> GetAll()
    {
        var resources = await _context.Resources
            .Where(r => r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(resources.Select(MapToDto));
    }


    // ADMIN: GET ALL (DRAFT + PUBLISHED)
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<ResourceDto>>> GetAllAdmin()
    {
        var resources = await _context.Resources
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(resources.Select(MapToDto));
    }


    // PUBLIC: GET BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetById(int id)
    {
        var resource = await _context.Resources.FindAsync(id);


        if (resource == null)
            return NotFound("Resource not found");


        return Ok(MapToDto(resource));
    }


    // ADMIN: CREATE
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ResourceDto>> Create(CreateResourceDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );


        var resource = new Resource
        {
            Title = dto.Title,
            Summary = dto.Summary,
            Description = dto.Description,
            Category = dto.Category,
            Link = dto.Link,
            ImageUrl = dto.ImageUrl,
            IsPublished = dto.IsPublished,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };


        _context.Resources.Add(resource);

        await _context.SaveChangesAsync();



        await _auditService.Log(
            userId,
            resource.IsPublished
                ? "Published Resource"
                : "Created Resource",
            $"Resource: \"{resource.Title}\""
        );

        // Notify students when a resource is published
        if (resource.IsPublished)
        {
            var students = await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .ToListAsync();


            foreach (var student in students)
            {
                await _notificationService.CreateAsync(
                    student.Id,
                    "New Resource Available",
                    $"A new resource \"{resource.Title}\" has been published.",
                    "Resource",
                    resource.Id
                );
            }
        }

        return Ok(MapToDto(resource));
    }


    // ADMIN: UPDATE
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        int id,
        UpdateResourceDto dto)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );


        var resource = await _context.Resources.FindAsync(id);


        if (resource == null)
            return NotFound("Resource not found");



        resource.Title = dto.Title;
        resource.Summary = dto.Summary;
        resource.Description = dto.Description;
        resource.Category = dto.Category;
        resource.Link = dto.Link;
        resource.ImageUrl = dto.ImageUrl;
        var wasPublished = resource.IsPublished;


        resource.Title = dto.Title;
        resource.Summary = dto.Summary;
        resource.Description = dto.Description;
        resource.Category = dto.Category;
        resource.Link = dto.Link;
        resource.ImageUrl = dto.ImageUrl;
        resource.IsPublished = dto.IsPublished;

        resource.UpdatedAt = DateTime.UtcNow;


        await _context.SaveChangesAsync();


        // Notify students when draft becomes published
        if (!wasPublished && resource.IsPublished)
        {
            var students = await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .ToListAsync();


            foreach (var student in students)
            {
                await _notificationService.CreateAsync(
                    student.Id,
                    "New Resource Available",
                    $"A new resource \"{resource.Title}\" has been published.",
                    "Resource",
                    resource.Id
                );
            }
        }

        await _auditService.Log(
            userId,
            "Updated Resource",
            $"Resource: \"{resource.Title}\""
        );

        return NoContent();
    }


    // ADMIN: DELETE
    [Authorize(Roles = "JuniorAdmin,SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );


        var resource = await _context.Resources.FindAsync(id);


        if (resource == null)
            return NotFound("Resource not found");



        var resourceTitle = resource.Title;



        _context.Resources.Remove(resource);

        await _context.SaveChangesAsync();




        await _auditService.Log(
            userId,
            "Deleted Resource",
            $"Resource: \"{resourceTitle}\""
        );



        return NoContent();
    }


    // MAPPER
    private static ResourceDto MapToDto(Resource r)
    {
        return new ResourceDto
        {
            Id = r.Id,
            Title = r.Title,
            Summary = r.Summary,
            Description = r.Description,
            Category = r.Category,
            Link = r.Link,
            ImageUrl = r.ImageUrl,
            IsPublished = r.IsPublished,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}