using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class UpdatePermissionDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string KeyName { get; set; } = string.Empty;

        [Required]
        public int ModuleId { get; set; }
    }
}
