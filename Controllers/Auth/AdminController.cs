using grad.Data;
using grad.Models;
using grad.DTOs;
using grad.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AdminController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> AdminLogin([FromBody] LoginRequest req)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
            return BadRequest("Email and password are required.");

        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null) return Unauthorized("Invalid credentials.");

        // Check if user is in Admin role
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            return Unauthorized("You are not an admin.");

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials.");

        // Generate JWT token
        var token = await _tokenService.CreateToken(user);

        return Ok(new
        {
            Token = token,
            user.Id,
            user.firstname,
            user.lastname,
            user.Email
        });
    }

    // ------------------- Pending Teachers -------------------
    [HttpGet("pending-teachers")]
    public async Task<IActionResult> GetPendingTeachers()
    {
        var pendingTeachers = await _db.Teachers
            .Include(t => t.User)
            .Where(t => !t.is_approved)
            .Select(t => new
            {
                t.User.Id,
                t.User.firstname,
                t.User.lastname,
                t.User.Email
            })
            .ToListAsync();

        return Ok(pendingTeachers);
    }

    // ------------------- Approve Teacher -------------------
    [HttpPut("approve-teacher/{id}")]
    public async Task<IActionResult> ApproveTeacher(Guid id)
    {
        var teacher = await _db.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.User.Id == id);

        if (teacher == null) return NotFound("Teacher not found.");

        teacher.is_approved = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Teacher approved successfully." });
    }

    // ------------------- Delete Teacher -------------------
    [HttpDelete("delete-teacher/{id}")]
    public async Task<IActionResult> DeleteTeacher(Guid id)
    {
        var teacher = await _db.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.User.Id == id);

        if (teacher == null) return NotFound("Teacher not found.");

        // Remove User (Identity) — this will cascade to Teacher entity
        await _userManager.DeleteAsync(teacher.User);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Teacher deleted successfully." });
    }

    // ------------------- All Teachers -------------------
    [HttpGet("all-teachers")]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _db.Teachers
            .Include(t => t.User)
            .Select(t => new
            {
                t.User.Id,
                t.User.firstname,
                t.User.lastname,
                t.User.Email,
                t.is_approved
            })
            .ToListAsync();

        return Ok(teachers);
    }
}
