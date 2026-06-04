using SpeakUp.API.Models.ReportModel;

namespace SpeakUp.API.DTOs.Report;

public class UpdateStatusDto
{
    public ReportStatus Status { get; set; }
}