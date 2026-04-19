using Backend.Models;
using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;

public class Permission : BaseStaticEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string KeyName { get; set; } = string.Empty;

    public ICollection<MenuPermission> MenuPermissions { get; set; }
}