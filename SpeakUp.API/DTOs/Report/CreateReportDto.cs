public class CreateReportDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }

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

    public List<string> ComplaintNature { get; set; } = new();

    public string? Witness1Name { get; set; }
    public string? Witness1Contact { get; set; }
    public string? Witness2Name { get; set; }
    public string? Witness2Contact { get; set; }

    public string? PriorReportWhere { get; set; }
    public string? DesiredOutcome { get; set; }

    public bool Confidential { get; set; }
}