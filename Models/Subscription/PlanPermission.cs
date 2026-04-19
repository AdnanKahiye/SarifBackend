using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Subscription
{
    public class PlanPermission : BaseEntity
    {



        [Required]
        public int PermissionId { get; set; }
        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; } = null!;

        [Required]
        public Guid PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;
    }
}
