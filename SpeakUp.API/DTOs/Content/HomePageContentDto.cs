using SpeakUp.API.Models.ContentModel;
namespace SpeakUp.API.DTOs.Content;

public class HomePageContentDto
{
    public int Id { get; set; }
    public ContentType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}