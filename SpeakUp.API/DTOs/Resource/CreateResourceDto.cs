using SpeakUp.API.Models.ResourceModel;

namespace SpeakUp.API.DTOs.Resource;

public class CreateResourceDto
{
    public required string Title { get; set; }

    public string? Summary { get; set; }

    public required string Description { get; set; }

    public required ResourceCategory Category { get; set; }

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; } = true;
}