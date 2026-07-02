using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class DutyAssignment
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public int ClientId { get; set; }   // Replaces LocationName
        [ForeignKey("ClientId")]
        public Client Client { get; set; } = null!;

        [Required]
        public int ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public Shift Shift { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; } // Assignment date

        public string? Notes { get; set; } // Optional notes about assignment
    }
}