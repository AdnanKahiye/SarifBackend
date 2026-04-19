using Microsoft.AspNetCore.Identity;

namespace Backend.Models.Identity
{
    public class ApplicationRole : IdentityRole

    {

        //public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
