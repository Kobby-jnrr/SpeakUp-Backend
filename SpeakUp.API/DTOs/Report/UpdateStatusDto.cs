using SpeakUp.API.Models.ReportModel;

namespace SpeakUp.API.DTOs.Report;

public class UpdateStatusDto
{
    public required ReportStatus Status { get; set; }
}