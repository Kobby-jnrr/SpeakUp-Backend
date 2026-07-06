using SpeakUp.API.Models.ResourceModel;

namespace SpeakUp.API.DTOs.Resource;

public class ResourceDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string? Summary { get; set; }

    public string Description { get; set; } = "";

    public ResourceCategory Category { get; set; }

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}