using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent
    }

    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        // Multiple shifts attended in a single day
        public List<AttendanceShift> AttendanceShifts { get; set; } = new List<AttendanceShift>();
    }
}