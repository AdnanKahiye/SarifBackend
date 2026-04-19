using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Setup
{
    public class Module : BaseStaticEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Menu> Menus { get; set; }
    }
}
