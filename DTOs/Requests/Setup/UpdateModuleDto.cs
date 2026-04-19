using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{

    public class UpdateModuleDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
