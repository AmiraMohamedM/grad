using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grad.Models
{
   
    
        public class Teacher
        {
        [Key]
        public Guid teacher_id { get; set; }

        [ForeignKey("User")]
        public Guid user_id { get; set; }

        public string bio { get; set; }
        public string subject { get; set; }

        public bool is_approved { get; set; } = false;

        public ApplicationUser User { get; set; }

    }
    }
