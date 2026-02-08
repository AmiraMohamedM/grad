using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace grad.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {

        public string firstname { get; set; }
        public string lastname { get; set; }


        public string? language_pref { get; set; }
        public string? device_id { get; set; }


        public bool? is_approved { get; set; }

        


        public string FullName => $"{firstname} {lastname}";
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }


    }

}
