using SpeakUp.API.Models.ContentModel;

namespace SpeakUp.API.DTOs.Content;

public class HomePageContentDto
{
    public int Id { get; set; }

    public ContentType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}