using grad.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace grad.Data
{
    // Use IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
  
        public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        

        // Additional domain tables
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<AcademicLevel> AcademicLevels { get; set; }
        public DbSet<ClassLevel> ClassLevels { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<UserStatistics> UserStatistics { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // MUST call for Identity tables

            // Map Student table
            modelBuilder.Entity<Student>()
                .ToTable("students")
                .HasKey(s => s.student_id);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.user_id);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.AcademicLevel)
                .WithMany()
                .HasForeignKey(s => s.academic_level_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Student → ClassLevel
            modelBuilder.Entity<Student>()
                .HasOne(s => s.ClassLevel)
                .WithMany()
                .HasForeignKey(s => s.class_level_id)
                .OnDelete(DeleteBehavior.Restrict);

            // ClassLevel → AcademicLevel
            modelBuilder.Entity<ClassLevel>()
                .HasOne(c => c.AcademicLevel)
                .WithMany(a => a.ClassLevels)
                .HasForeignKey(c => c.academic_level_id)
                .OnDelete(DeleteBehavior.Cascade);


            // Map Teacher table
            modelBuilder.Entity<Teacher>()
                .ToTable("teachers")
                .HasKey(t => t.teacher_id);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.user_id);


            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }
    }
}
