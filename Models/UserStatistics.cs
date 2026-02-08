using System.ComponentModel.DataAnnotations;

namespace grad.Models
{
    public class UserStatistics
    {
        [Key]
        public int Id { get; set; }

        public Guid StudentId { get; set; } // ده المهم (Guid)

        public int Absence { get; set; } // كان ناقص
        public int Tasks { get; set; }   // كان ناقص
        public int Quiz { get; set; }    // كان ناقص
    }
}