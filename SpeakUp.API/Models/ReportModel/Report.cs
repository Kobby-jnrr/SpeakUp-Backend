using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.ReportModel;

public class Report
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;


    public int? AssignedAdminId { get; set; }
    public User? AssignedAdmin { get; set; }

    public int? LastModifiedById { get; set; }
    public User? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public int? PreviousAdminId { get; set; }
    public User? PreviousAdmin { get; set; }
    public DateTime? ReassignedAt { get; set; }

 
    public string? ComplainantGender { get; set; }
    public string? ComplainantStudentId { get; set; }
    public string? Department { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }


    public string? RespondentName { get; set; }
    public string? RespondentPosition { get; set; }
    public string? RespondentDepartment { get; set; }
    public string? RelationshipToComplainant { get; set; }


    public string? IncidentDate { get; set; }
    public string? IncidentTime { get; set; }
    public string? IncidentLocation { get; set; }

    public string? ComplaintNature { get; set; } 


    public string? Witness1Name { get; set; }
    public string? Witness1Contact { get; set; }
    public string? Witness2Name { get; set; }
    public string? Witness2Contact { get; set; }

    
    public string? PriorReportWhere { get; set; }

 
    public string? DesiredOutcome { get; set; }

    public bool Confidential { get; set; } = false;
}