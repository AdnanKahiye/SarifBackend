using Backend.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Setup
{
    public class RolePermission : BaseStaticEntity
    {

        [Required]
        public string RoleId { get; set; } = string.Empty;
        [ForeignKey(nameof(RoleId))]
        public ApplicationRole Role { get; set; } = null!;

        [Required]
        public int PermissionId { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;

    }
}
