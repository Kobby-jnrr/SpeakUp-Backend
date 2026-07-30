using SpeakUp.API.Models.UserModel;

namespace SpeakUp.API.DTOs.Auth;

public class CreateAdminInvitationDto
{
    public required string Email { get; set; }

    public UserRole Role { get; set; }
}