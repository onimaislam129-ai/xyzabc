using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for your models
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceShift> AttendanceShifts { get; set; }
        public DbSet<DutyAssignment> DutyAssignments { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Enums to be stored as strings
            builder.Entity<ApplicationUser>()
                .Property(u => u.Position)
                .HasConversion<string>();

            builder.Entity<Employee>()
                .Property(e => e.Position)
                .HasConversion<string>();

            builder.Entity<Client>()
                .Property(c => c.Type)
                .HasConversion<string>();

            builder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<string>();

            // Relationships
            builder.Entity<Attendance>()
                .HasMany(a => a.AttendanceShifts)
                .WithOne(asf => asf.Attendance)
                .HasForeignKey(asf => asf.AttendanceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Salary>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DutyAssignment>()
                .HasOne(d => d.Employee)
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DutyAssignment>()
                .HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DutyAssignment>()
                .HasOne(d => d.Shift)
                .WithMany()
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Salary precision
            builder.Entity<Salary>()
                .Property(s => s.TotalSalary)
                .HasPrecision(18, 2);
        }
    }
}