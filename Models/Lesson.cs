using System.ComponentModel.DataAnnotations;

namespace grad.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // ده كان ناقص
        public int LessonCount { get; set; } // ده كان ناقص
        public string Duration { get; set; } = "0h 0m"; // ده كان ناقص
        public decimal Rating { get; set; }
        public string ImageUrl { get; set; } = string.Empty; // ده كان ناقص
    }
}