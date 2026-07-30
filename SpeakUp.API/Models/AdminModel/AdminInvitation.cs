using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.Models.AdminModel;

public class AdminInvitation
{
    public int Id { get; set; }


    // Email address of the person receiving the invitation
    public required string Email { get; set; }


    // The role they will receive after signup
    public UserRole Role { get; set; }


    // Unique signup token
    public required string Token { get; set; }


    // SuperAdmin who generated the invitation
    public int CreatedBy { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // When the link becomes invalid
    public DateTime ExpiryDate { get; set; }


    // Prevent reuse
    public bool IsUsed { get; set; } = false;
}