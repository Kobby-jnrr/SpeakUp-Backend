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

    public AuthController(
        ApplicationDbContext context,
        TokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
    }


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


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == dto.Email);

        if (user == null)
            return BadRequest("Invalid email or password");

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!isPasswordValid)
            return BadRequest("Invalid email or password");

        var token = _tokenService.CreateToken(user);

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


    [AllowAnonymous]
    [HttpPost("setup-superadmin")]
    public async Task<IActionResult> SetupSuperAdmin(CreateSuperAdminDto dto)
    {
        var existingAdmin = await _context.Users
            .AnyAsync(u => u.Role == UserRole.SuperAdmin);

        if (existingAdmin)
            return BadRequest("SuperAdmin already exists.");

        var setupKey = _configuration["AdminSetup:SetupKey"];

        if (string.IsNullOrEmpty(setupKey) || dto.SetupKey != setupKey)
            return Unauthorized("Invalid setup key.");

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

        return Ok(new
        {
            message = "SuperAdmin created successfully"
        });
    }


    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("create-junior-admin")]
    public async Task<IActionResult> CreateJuniorAdmin(CreateJuniorAdminDto dto)
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

        return Ok(new
        {
            message = "Junior Admin created successfully",
            admin.Id,
            admin.Email,
            admin.Role
        });
    }
}