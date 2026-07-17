public class ConversationListDto
{
    public int Id { get; set; }

    public string ChatType { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsAnonymous { get; set; }

    public DateTime CreatedAt { get; set; }

    public int UnreadCount { get; set; }

    public int? ReportId { get; set; }
    public string? ReportCode { get; set; }

    public int StudentId { get; set; }
    public string? StudentName { get; set; }

    public int? AssignedAdminId { get; set; }
    public string? AssignedAdminName { get; set; }

    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
}