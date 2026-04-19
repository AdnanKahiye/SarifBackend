using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Setup
{
    public class CreateRolePermissionDto
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }
}
