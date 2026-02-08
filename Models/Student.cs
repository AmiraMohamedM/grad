using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grad.Models
{

    public class Student
    {
        [Key]
        public Guid student_id { get; set; }

        [ForeignKey("User")]
        public Guid user_id { get; set; }

        public int? academic_level_id { get; set; }
        public int? class_level_id { get; set; }

        public string parent_email { get; set; }

        public AcademicLevel AcademicLevel { get; set; }
        public ClassLevel ClassLevel { get; set; }
        public ApplicationUser User { get; set; }
    }
}

