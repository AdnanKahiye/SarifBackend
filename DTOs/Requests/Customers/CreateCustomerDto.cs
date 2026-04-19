using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Customers
{
    public class CreateCustomerDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public byte? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        [Required, MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? AltPhoneNumber { get; set; }

        public string? Address { get; set; } = "Somalia";


    }
}
