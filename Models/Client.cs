using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public enum ClientType
    {
        HousingComplex,
        Factory
    }

    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        [Phone]
        public string Phone { get; set; } = null!;

        public string? Email { get; set; }

        [Required]
        public ClientType Type { get; set; } // Will be saved as string
    }
}