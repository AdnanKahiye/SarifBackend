using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Setup
{
    public class MenuPermission : BaseStaticEntity
    {

    

        [Required]
        public int MenuId { get; set; }
        // 🔹 Navigation
        [ForeignKey(nameof(MenuId))]
        public Menu Menu { get; set; } = null!;

        [Required]
        public int PermissionId { get; set; }
        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;

    }
}
