using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.ReportModel
{
    public class Report
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public ReportStatus Status { get; set; }
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
    }
}