using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class CreateModuleDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
