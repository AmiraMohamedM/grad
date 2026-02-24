using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grad.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Teacher")]
        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        // Basic Info
        public string Title { get; set; } = string.Empty; // e.g., "Basics of Algebra"
        public string Category { get; set; } = string.Empty; // e.g., "Math"
        public string Introduction { get; set; } = string.Empty; // Description of the course
        public string VideoUrl { get; set; } = string.Empty; // Promo video link

        // Subject Page Details
        public string Schedule { get; set; } = string.Empty;
        public string ClassType { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }

        // Relationship: A course has many sessions
        public ICollection<CourseSession> Sessions { get; set; }
    }
}
