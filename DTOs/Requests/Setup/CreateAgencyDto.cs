using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class CreateAgencyDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }
    }
}
