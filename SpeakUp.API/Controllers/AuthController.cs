using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Auth;
using SpeakUp.API.Models.UserModel;
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

    public AuthController(
        ApplicationDbContext context,
        TokenService tokenService,
        IConfiguration configuration,
        AuditService auditService)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _auditService = auditService;
    }


    // STUDENT REGISTER
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (existingUser != null)
            return BadRequest("User already exists");


        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender,
            Department = dto.Department,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Student
        };


        _context.Users.Add(user);

        await _context.SaveChangesAsync();


        return Ok(new
        {
            message = "User registered successfully",
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.Gender,
            user.Role
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
            Role = UserRole.SuperAdmin
        };


        _context.Users.Add(admin);

        await _context.SaveChangesAsync();



        // No previous admin exists, so system log
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


    // CREATE JUNIOR ADMIN
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("create-junior-admin")]
    public async Task<IActionResult> CreateJuniorAdmin(CreateAdminDto dto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);


        if (existingUser != null)
            return BadRequest("User already exists");



        var admin = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.JuniorAdmin
        };


        _context.Users.Add(admin);

        await _context.SaveChangesAsync();



        var creatorId = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
        );



        await _auditService.Log(
            creatorId,
            "Created Junior Admin",
            admin.Email
        );



        return Ok(new
        {
            message = "Junior Admin created successfully",
            admin.Id,
            admin.Email,
            admin.Role
        });
    }

    // CREATE SUPER ADMIN
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("create-super-admin")]
    public async Task<IActionResult> CreateSuperAdmin(CreateAdminDto dto)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);


        if (existingUser != null)
            return BadRequest("User already exists");



        var superAdminCount = await _context.Users
            .CountAsync(u => u.Role == UserRole.SuperAdmin);



        if (superAdminCount >= 3)
            return BadRequest("Maximum SuperAdmins reached.");



        var admin = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.SuperAdmin
        };



        _context.Users.Add(admin);

        await _context.SaveChangesAsync();



        var creatorId = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value
        );


        await _auditService.Log(
            creatorId,
            "Created Super Admin",
            admin.Email
        );


        return Ok(new
        {
            message = "Super Admin created successfully",
            admin.Id,
            admin.Email,
            admin.Role
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



        // Prevent deleting SuperAdmins
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