using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Salary
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [NotMapped] // EF Core will ignore this property in the database
        public int TotalShifts { get; set; }

        [NotMapped]
        public int AbsentShifts { get; set; }

        [NotMapped]
        public decimal EarningBeforeDeduction { get; set; } // TotalShifts * per-shift rate

        public decimal TotalSalary { get; set; } // Final salary after deduction
    }
}