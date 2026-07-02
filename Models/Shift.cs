using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class Shift
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Morning, Evening, Night

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public TimeSpan BreakTime { get; set; }
    }
}