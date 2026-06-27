namespace SpeakUp.API.DTOs.Report;

public class CreateQuickReportDto
{
    public string Description { get; set; } = null!;
    public bool IsAnonymous { get; set; }
}