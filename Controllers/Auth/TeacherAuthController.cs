using grad.Data;
using grad.DTOs;
using grad.Models;
using grad.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/auth/teacher")]
public class TeacherAuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public TeacherAuthController(
        AppDbContext db,
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _db = db;
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ------------------- Register -------------------
    [HttpPost("register")]
    public async Task<IActionResult> RegisterTeacher(RegisterTeacherRequest req)
    {
        var existing = await _userManager.FindByEmailAsync(req.Email);
        if (existing != null) return BadRequest("Email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = req.Email,
            Email = req.Email,
            firstname = req.FirstName,
            lastname = req.LastName,
            language_pref = "en",
            device_id = "",
            is_approved = false
        };

        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "Teacher");

        var teacher = new Teacher
        {
            teacher_id = Guid.NewGuid(),
            user_id = user.Id,
            bio = req.Bio,
            subject = req.Subject,
            is_approved = false
        };
        _db.Teachers.Add(teacher);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Teacher registered. Pending admin approval." });
    }

    // ------------------- Login -------------------
    [HttpPost("login")]
    public async Task<IActionResult> LoginTeacher(LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null) return Unauthorized("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Teacher")) return Unauthorized("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials.");

        var teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.user_id == user.Id);
        if (!teacher.is_approved) return Unauthorized("Your account is pending admin approval.");

        var token = await _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Role = "Teacher",
            Firstname = user.firstname,
            Lastname = user.lastname,
            Email = user.Email
        });
    }

    // ------------------- Forgot Password -------------------
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Email is required.");

        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null) return Ok(new { message = "If that email exists, a reset link has been sent." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        Console.WriteLine($"Teacher reset token for {user.Email}: {token}");

        return Ok(new { message = "Password reset link sent if email exists." });
    }

    // ------------------- Reset Password -------------------
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest("Token and new password are required.");

        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null) return BadRequest("Invalid token.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Teacher")) return BadRequest("Invalid token.");

        var result = await _userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { message = "Password has been reset successfully." });
    }

    // ------------------- Google Login -------------------
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleResponse))
        };

        return Challenge(props, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return BadRequest("Error loading external login info");

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        ApplicationUser user;

        if (!signInResult.Succeeded)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                firstname = name.Split(' ')[0],
                lastname = name.Split(' ').Length > 1 ? name.Split(' ')[1] : "",
                is_approved = false // teachers need admin approval
            };

            await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, info);
            await _userManager.AddToRoleAsync(user, "Teacher");

            var teacher = new Teacher
            {
                teacher_id = Guid.NewGuid(),
                user_id = user.Id,
                bio = "",
                subject = "",
                is_approved = false
            };
            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();
        }
        else
        {
            user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }

        var token = await _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Role = "Teacher",
            Firstname = user.firstname,
            Lastname = user.lastname,
            Email = user.Email
        });
    }
}
