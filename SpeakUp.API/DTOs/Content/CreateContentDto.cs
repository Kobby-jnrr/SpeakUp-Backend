using SpeakUp.API.Models.ContentModel;

namespace SpeakUp.API.DTOs.Content;

public class CreateContentDto
{
    public ContentType Type { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }
}