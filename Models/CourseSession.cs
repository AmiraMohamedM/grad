using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grad.Models
{
    public class CourseSession
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        // Relationship with Course table
        public Course Course { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty; // e.g., "Session 1: Introduction"

        public string Duration { get; set; } = string.Empty; // e.g., "45 mins"

        public string Description { get; set; } = string.Empty; // Introduction for the session

        public string VideoUrl { get; set; } = string.Empty; // Video link for the session

        public string HomeworkUrl { get; set; } = string.Empty; // Link to homework (PDF or Quiz)

        public bool IsLocked { get; set; } = true; // To lock videos until the student buys the course
    }
}  