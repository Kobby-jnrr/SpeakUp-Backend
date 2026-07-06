namespace SpeakUp.API.Models.ResourceModel;

public class Resource
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? Summary { get; set; }

    public required string Description { get; set; }

    public required ResourceCategory Category { get; set; }

    public string? Link { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
