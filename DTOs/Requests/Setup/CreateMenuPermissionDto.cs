using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class CreateMenuPermissionDto
    {
        [Required]
        public int MenuId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }
}
