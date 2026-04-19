using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Setup
{
    public class Menu : BaseStaticEntity
    {

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Href { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        // 🔹 Self Reference
        public int? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public Menu? Parent { get; set; }

        public ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<MenuPermission> MenuPermissions { get; set; }

        public int OrderNo { get; set; } = 0;

        public int ModuleId { get; set; }   // ✅ ADD THIS

        [ForeignKey(nameof(ModuleId))]
        public Module Module { get; set; } = null!;


    }
}
