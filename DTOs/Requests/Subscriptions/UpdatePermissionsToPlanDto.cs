using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Subscriptions
{
    public class UpdatePermissionsToPlanDto
    {
        [Required]
        public Guid PlanId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }
}
