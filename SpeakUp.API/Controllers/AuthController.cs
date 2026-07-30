using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Auth;
using SpeakUp.API.Models.UserModel;
using SpeakUp.API.Models.AdminModel;
using SpeakUp.API.Services;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly AuditService _auditService;
    private readonly EmailService _emailService;

    public AuthController(
        ApplicationDbContext context,
        TokenService tokenService,
        IConfiguration configuration,
        AuditService auditService,
        EmailService emailService)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _auditService = auditService;
        _emailService = emailService;
    }


    // STUDENT REGISTER
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser = await _context.Users
    .FirstOrDefaultAsync(x => x.Email == dto.Email);


        if (existingUser != null)
        {
            if (!existingUser.EmailVerified)
            {
                return BadRequest(
                    "Account exists but email is not verified."
                );
            }

            return BadRequest("User already exists");
        }


        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender,
            Department = dto.Department,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Student,

            EmailVerified = false,

            EmailVerificationCode =
            Random.Shared.Next(100000, 999999).ToString(),

            EmailVerificationExpiry =
            DateTime.UtcNow.AddMinutes(15)
        };


        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendVerificationEmail(
                    user.Email,
                    user.EmailVerificationCode!
                );

                Console.WriteLine("Verification email sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"EMAIL ERROR: {ex.Message}"
                );
            }
        });

        return Ok(new
        {
            message =
            "Registration successful. Check your email to verify your account."
        });
    }

    // VERIFY EMAIL
    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
            return BadRequest("Account not found.");

        if (user.EmailVerified)
            return BadRequest("Email already verified.");

        if (user.EmailVerificationExpiry < DateTime.UtcNow)
            return BadRequest("Verification code expired.");

        if (user.EmailVerificationCode != dto.Code)
            return BadRequest("Invalid verification code.");


        user.EmailVerified = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationExpiry = null;


        await _context.SaveChangesAsync();


        return Ok(new
        {
            message = "Email verified successfully."
        });
    }

    // RESEND VERIFICATION CODE
    [AllowAnonymous]
    [HttpPost("resend-verification-code")]
    public async Task<IActionResult> ResendVerificationCode(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            return BadRequest("Account not found.");
        }

        if (user.EmailVerified)
        {
            return BadRequest("Email is already verified.");
        }

        user.EmailVerificationCode =
            Random.Shared.Next(100000, 999999).ToString();

        user.EmailVerificationExpiry =
            DateTime.UtcNow.AddMinutes(15);

        await _context.SaveChangesAsync();

        try
        {
            Console.WriteLine("Starting resend email...");

            await _emailService.SendVerificationEmail(
                user.Email,
                user.EmailVerificationCode!
            );

            Console.WriteLine("Resend email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Failed to resend verification email: {ex.Message}"
            );
        }

        return Ok(new
        {
            message = "A new verification code has been sent to your email."
        });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    ForgotPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);


        // Always return same response
        if (user == null)
        {
            return Ok(new
            {
                message =
                "If the account exists, a reset link has been sent."
            });
        }


        var token =
            Guid.NewGuid().ToString();


        user.PasswordResetToken = token;

        user.PasswordResetExpiry =
            DateTime.UtcNow.AddMinutes(15);


        await _context.SaveChangesAsync();


        await _emailService.SendPasswordResetEmail(
            user.Email,
            token
        );


        return Ok(new
        {
            message =
            "If the account exists, a reset link has been sent."
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
    ResetPasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);


        if (user == null)
        {
            return BadRequest(
                "Invalid reset request."
            );
        }


        if (user.PasswordResetToken != dto.Token)
        {
            return BadRequest(
                "Invalid reset token."
            );
        }


        if (user.PasswordResetExpiry < DateTime.UtcNow)
        {
            return BadRequest(
                "Reset link expired."
            );
        }


        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(
                dto.NewPassword
            );


        user.PasswordResetToken = null;

        user.PasswordResetExpiry = null;


        await _context.SaveChangesAsync();


        return Ok(new
        {
            message =
            "Password reset successful."
        });
    }

    // LOGIN
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);


        if (user == null)
            return BadRequest("Invalid email or password");

        if (!user.EmailVerified)
        {
            return BadRequest(new
            {
                code = "EMAIL_NOT_VERIFIED",
                message = "Email not verified."
            });
        }


        var isPasswordValid =
            BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);


        if (!isPasswordValid)
            return BadRequest("Invalid email or password");


        var token = _tokenService.CreateToken(user);


        // AUDIT ADMIN LOGIN
        if (user.Role == UserRole.JuniorAdmin ||
            user.Role == UserRole.SuperAdmin)
        {
            await _auditService.Log(
                user.Id,
                "Logged In",
                "Successful login"
            );
        }

        return Ok(new
        {
            message = "Login successful",
            token,
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.Gender,
            user.Department,
            user.Role
        });
    }


    // FIRST SUPER ADMIN CREATION
    [AllowAnonymous]
    [HttpPost("setup-superadmin")]
    public async Task<IActionResult> SetupSuperAdmin(CreateSuperAdminDto dto)
    {
        var existingAdmin = await _context.Users
            .AnyAsync(u => u.Role == UserRole.SuperAdmin);


        if (existingAdmin)
            return BadRequest("SuperAdmin already exists.");

        var setupKey = _configuration["AdminSetup:SetupKey"];


        if (string.IsNullOrEmpty(setupKey) ||
            dto.SetupKey != setupKey)
        {
            return Unauthorized("Invalid setup key.");
        }

        var admin = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.SuperAdmin,
            EmailVerified = true
        };

        _context.Users.Add(admin);

        await _context.SaveChangesAsync();

        await _auditService.Log(
            admin.Id,
            "Created Super Admin",
            admin.Email
        );

        return Ok(new
        {
            message = "SuperAdmin created successfully"
        });
    }

    // CREATE ADMIN INVITATION
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("create-admin-invitation")]
    public async Task<IActionResult> CreateAdminInvitation(
        CreateAdminInvitationDto dto)
    {

        // Check email already exists
        var existingUser = await _context.Users
            .AnyAsync(x => x.Email == dto.Email);


        if (existingUser)
        {
            return BadRequest(
                "An account with this email already exists."
            );
        }


        // Check if invitation already exists
        var existingInvitation =
            await _context.AdminInvitations
            .FirstOrDefaultAsync(x =>
                x.Email == dto.Email &&
                !x.IsUsed
            );


        if (existingInvitation != null)
        {
            return BadRequest(
                "An active invitation already exists for this email."
            );
        }



        // Limit SuperAdmins
        if (dto.Role == UserRole.SuperAdmin)
        {
            var superAdminCount =
                await _context.Users
                .CountAsync(x => x.Role == UserRole.SuperAdmin);


            if (superAdminCount >= 3)
            {
                return BadRequest(
                    "Maximum SuperAdmins reached."
                );
            }
        }



        var token =
            Guid.NewGuid()
            .ToString();



        var creatorId = int.Parse(
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )!.Value
        );



        var invitation = new AdminInvitation
        {
            Email = dto.Email,

            Role = dto.Role,

            Token = token,

            CreatedBy = creatorId,

            CreatedAt = DateTime.UtcNow,

            ExpiryDate =
                DateTime.UtcNow.AddHours(24),

            IsUsed = false
        };



        _context.AdminInvitations.Add(invitation);


        await _context.SaveChangesAsync();



        await _auditService.Log(
            creatorId,
            "Created Admin Invitation",
            $"{dto.Email} - {dto.Role}"
        );



        var frontendUrl =
            _configuration["FrontendUrl"];



        var signupLink =
            $"{frontendUrl}/admin-signup?token={token}";

        await _emailService.SendAdminInvitationEmail(
            dto.Email,
            dto.Role.ToString(),
            signupLink
        );



        return Ok(new
        {
            message =
            "Admin invitation created successfully.",

            email = dto.Email,

            role = dto.Role,

            signupLink
        });
    }

    // VALIDATE ADMIN INVITATION
    [AllowAnonymous]
    [HttpGet("validate-admin-invitation/{token}")]
    public async Task<IActionResult> ValidateAdminInvitation(
        string token)
    {
        var invitation =
            await _context.AdminInvitations
            .FirstOrDefaultAsync(x =>
                x.Token == token
            );


        if (invitation == null)
        {
            return BadRequest(
                "Invalid invitation."
            );
        }


        if (invitation.IsUsed)
        {
            return BadRequest(
                "This invitation has already been used."
            );
        }


        if (invitation.ExpiryDate < DateTime.UtcNow)
        {
            return BadRequest(
                "This invitation has expired."
            );
        }


        return Ok(new
        {
            email = invitation.Email,

            role = invitation.Role
        });
    }

    // COMPLETE ADMIN REGISTRATION
    [AllowAnonymous]
    [HttpPost("complete-admin-registration")]
    public async Task<IActionResult> CompleteAdminRegistration(
        CompleteAdminRegistrationDto dto)
    {

        var invitation =
            await _context.AdminInvitations
            .FirstOrDefaultAsync(x =>
                x.Token == dto.Token
            );


        if (invitation == null)
        {
            return BadRequest(
                "Invalid invitation."
            );
        }


        if (invitation.IsUsed)
        {
            return BadRequest(
                "Invitation already used."
            );
        }


        if (invitation.ExpiryDate < DateTime.UtcNow)
        {
            return BadRequest(
                "Invitation expired."
            );
        }



        var existingUser =
            await _context.Users
            .AnyAsync(x =>
                x.Email == invitation.Email
            );


        if (existingUser)
        {
            return BadRequest(
                "Account already exists."
            );
        }



        var user = new User
        {
            FirstName = dto.FirstName,

            LastName = dto.LastName,

            Email = invitation.Email,

            PhoneNumber = dto.PhoneNumber,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.Password
                ),

            Role = invitation.Role,

            EmailVerified = true
        };



        _context.Users.Add(user);



        // Mark invitation as used
        invitation.IsUsed = true;



        await _context.SaveChangesAsync();



        await _auditService.Log(
            user.Id,
            "Created Admin Account",
            $"{user.Email} - {user.Role}"
        );



        return Ok(new
        {
            message =
            "Admin account created successfully."
        });
    }



    // DELETE USER
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("delete-user/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var claim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        );

        if (claim == null)
            return Unauthorized();


        var currentAdminId = int.Parse(claim.Value);


        // Prevent deleting yourself
        if (currentAdminId == id)
        {
            return BadRequest(
                "You cannot delete your own account."
            );
        }


        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);


        if (user == null)
        {
            return NotFound("User not found");
        }



        if (user.Role == UserRole.SuperAdmin)
        {
            return BadRequest(
                "SuperAdmin accounts cannot be deleted."
            );
        }

        var deletedEmail = user.Email;
        var deletedRole = user.Role.ToString();

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        await _auditService.Log(
            currentAdminId,
            "Deleted User",
            $"Deleted {deletedRole}: {deletedEmail}"
        );



        return Ok(new
        {
            message = "User deleted successfully",
            deletedUser = deletedEmail,
            role = deletedRole
        });
    }
}