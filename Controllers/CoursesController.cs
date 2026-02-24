using grad.Data;
using grad.DTOs;
using grad.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace grad.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. API: Get Courses for Subject Page (GET)
        // Includes filtering by category (Math, Science, etc.)
        // ==========================================
        [HttpGet("subject-courses")]
        public async Task<IActionResult> GetCoursesByCategory([FromQuery] string category)
        {
            // Fetch courses and include related Teacher and User data
            var query = _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Where(c => c.Teacher.is_approved == true) // Only show courses from approved teachers
                .AsQueryable();

            // Filter by category if provided in the query string
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.Category.ToLower() == category.ToLower());
            }

            // Shape the data to match the Subject Page UI cards
            var coursesList = await query.Select(c => new
            {
                CourseId = c.Id,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher.User.firstname + " " + c.Teacher.User.lastname,
                Category = c.Category,
                Schedule = c.Schedule,
                ClassType = c.ClassType,
                MonthlyPrice = c.MonthlyPrice,
                IsFavorite = false // Placeholder for future Favorites feature
            }).ToListAsync();

            return Ok(coursesList);
        }

        // ==========================================
        // 2. API: Teacher adds a new Course (POST)
        // Requires a valid Teacher JWT Token
        // ==========================================
        [HttpPost("create-course")]
        [Authorize]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            // Extract the User ID from the Token claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Unauthorized. Please log in first.");
            }

            // Verify that the user has a registered Teacher profile
            var currentTeacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.user_id.ToString() == userIdString);

            if (currentTeacher == null)
            {
                return BadRequest("No teacher profile found for this user.");
            }

            // Create the new course entity
            var newCourse = new Course
            {
                TeacherId = currentTeacher.teacher_id,
                Title = dto.Title,
                Category = dto.Category,
                Introduction = dto.Introduction,
                VideoUrl = dto.VideoUrl,
                Schedule = dto.Schedule,
                ClassType = dto.ClassType,
                MonthlyPrice = dto.MonthlyPrice
            };

            _context.Courses.Add(newCourse);
            await _context.SaveChangesAsync();

            return Ok("Course created successfully!");
        }

        // ==========================================
        // 3. API: Get Course Details (GET)
        // Fetches full details including sessions when a card is clicked
        // ==========================================
        [HttpGet("course-details/{courseId}")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            // Fetch course by ID including Teacher, User, and Sessions (Lessons)
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Include(c => c.Sessions)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Return detailed object for the Course Details screen
            var courseDetails = new
            {
                CourseId = course.Id,
                Title = course.Title,
                TeacherName = course.Teacher.User.firstname + " " + course.Teacher.User.lastname,
                Category = course.Category,
                Introduction = course.Introduction,
                VideoUrl = course.VideoUrl,
                Schedule = course.Schedule,
                ClassType = course.ClassType,
                MonthlyPrice = course.MonthlyPrice,

                // Map list of sessions inside the course
                Sessions = course.Sessions.Select(s => new
                {
                    
                        SessionId = s.Id,
                        Title = s.Title,
                        Duration = s.Duration,
                        Description = s.Description, // New
                        VideoUrl = s.VideoUrl,       // New
                        HomeworkUrl = s.HomeworkUrl, // New
                        IsLocked = s.IsLocked
                    
                }).ToList()
            };

            return Ok(courseDetails);
        }

        // ==========================================
        // 4. API: Teacher adds a Session to a Course (POST)
        // Requires a valid Teacher JWT Token
        // ==========================================
        [HttpPost("add-session")]
        [Authorize]
        public async Task<IActionResult> AddSession([FromBody] CreateSessionDto dto)
        {
            // Validate that the course exists
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Create new session linked to the specified course
            var newSession = new CourseSession
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Duration = dto.Duration,
                Description = dto.Description, // Added
                VideoUrl = dto.VideoUrl,       // Added
                HomeworkUrl = dto.HomeworkUrl, // Added
                IsLocked = true
            };

            _context.CourseSessions.Add(newSession);
            await _context.SaveChangesAsync();

            return Ok("Session added successfully!");
        }

        // ==========================================
        // 5. API: Get Teachers' IDs (GET)
        // Utility endpoint for testing and mapping
        // ==========================================
        [HttpGet("get-teachers-ids")]
        public IActionResult GetTeachersIds()
        {
            var teachers = _context.Teachers
                .Select(t => new
                {
                    TeacherName = t.User.firstname + " " + t.User.lastname,
                    CorrectTeacherId = t.teacher_id
                })
                .ToList();

            return Ok(teachers);
        }
    }
}