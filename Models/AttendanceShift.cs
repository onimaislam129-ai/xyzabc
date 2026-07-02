using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class AttendanceShift
    {
        public int Id { get; set; }

        [Required]
        public int AttendanceId { get; set; }
        [ForeignKey("AttendanceId")]
        public Attendance Attendance { get; set; } = null!;

        [Required]
        public int ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public Shift Shift { get; set; } = null!;
    }
}