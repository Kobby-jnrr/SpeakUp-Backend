namespace SpeakUp.API.DTOs.Auth;

public class CompleteAdminRegistrationDto
{
    public required string Token { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string PhoneNumber { get; set; }

    public required string Password { get; set; }
}