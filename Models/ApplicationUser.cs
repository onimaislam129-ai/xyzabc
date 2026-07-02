using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public enum UserPosition
    {
        Admin,
        Guard
    }

    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserPosition Position { get; set; } // Enum instead of string
    }
}