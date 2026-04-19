using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class UpdateBranchDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Location { get; set; }

        public bool IsMain { get; set; }
    }
}
