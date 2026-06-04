using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.ContentModel;

public class HomePageContent
{
    public int Id { get; set; }

    public ContentType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }
}