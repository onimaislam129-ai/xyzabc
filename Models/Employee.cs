using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public enum EmployeePosition
    {
        Guard,
        Admin
    }

    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public string? Address { get; set; } // optional

        [Required]
        public EmployeePosition Position { get; set; } // Enum instead of string

        [Required]
        public DateTime JoiningDate { get; set; }
    }
}