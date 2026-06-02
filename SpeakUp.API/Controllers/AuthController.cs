using Microsoft.AspNetCore.Mvc;
using SpeakUp.API.Data;
using SpeakUp.API.DTOs.Auth;
using SpeakUp.API.Models.UserModel;
using SpeakUp.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace SpeakUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(ApplicationDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser = _context.Users.FirstOrDefault(x => x.Email == dto.Email);

        if (existingUser != null)
            return BadRequest("User already exists");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
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
            user.Role
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == dto.Email);

        if (user == null)
            return BadRequest("Invalid email or password");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

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
            user.Role
        });
    }
}